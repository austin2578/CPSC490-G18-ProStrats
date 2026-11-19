using System;
using System.Collections.Generic;

namespace ProjectRogue.Run
{
    public enum VisitState { Entered, Completed }

    // One instance of a NodeDefinition during this run
    public sealed class NodeVisit
    {
        private readonly HashSet<int> taken = new HashSet<int>();

        public NodeVisit(NodeDefinition definition, string instanceId)
        {
            Definition = definition;
            InstanceId = instanceId;
            State = VisitState.Entered;
        }

        public NodeDefinition Definition { get; }
        public string InstanceId { get; }
        public VisitState State { get; internal set; }
        public int ChoicesTaken => taken.Count;
        public bool HasTaken(int index) => taken.Contains(index);
        internal void MarkTaken(int index) => taken.Add(index);
    }

    // The rules of a single run: enter nodes, pick choices, complete nodes, apply rewards, spend attribute/mastery pts
    public sealed class RunSession : IRunStats
    {
        private readonly Dictionary<string, NodeVisit> visits = new Dictionary<string, NodeVisit>();
        private readonly Func<AttributeType, int> baseline;

        public event Action<GrantedReward> RewardGranted;
        public event Action AttributesChanged;      // allocation or respec; push totals to AttributeSet
        public event Action<int> HealRequested;     // amount of Health healed
        public event Action<NodeVisit> NodeEntered;
        public event Action<NodeVisit> NodeCompleted;

        public RunSession(RunTuning tuning, int seed, Func<AttributeType, int> baselineAttribute = null)
        {
            Seed = seed;
            Rng = new System.Random(seed);
            Progress = new RunProgress(tuning);
            baseline = baselineAttribute;
        }

        public int Seed { get; }
        public System.Random Rng { get; }
        public RunProgress Progress { get; }
        public NodeVisit CurrentVisit { get; private set; }
        public bool InNode => CurrentVisit != null && CurrentVisit.State == VisitState.Entered;

        // IRunStats
        public int Level => Progress.Level;
        public int Gold => Progress.Gold;
        public int UnspentMastery => Progress.UnspentMastery;
        public int GetBaseline(AttributeType type) => baseline != null ? baseline(type) : 0;
        public int GetAttribute(AttributeType type) => GetBaseline(type) + Progress.GetAllocated(type);
        public int GetMasteryInvested(string family) => Progress.GetMasteryInvested(family);

        // Routing
        public bool IsCompleted(string instanceId) => Progress.IsNodeCompleted(instanceId);

        public bool CanEnter(NodeDefinition node, string instanceId, out string reason)
        {
            reason = null;
            if (node == null) { reason = "No node"; return false; }
            if (!string.IsNullOrEmpty(instanceId) && Progress.IsNodeCompleted(instanceId))
            {
                reason = "Already cleared";
                return false;
            }
            if (InNode && CurrentVisit.InstanceId != instanceId)
            {
                reason = $"Finish {CurrentVisit.Definition.displayName} first";
                return false;
            }
            return RunRequirement.AllMet(node.entryRequirements, this, out reason);
        }

        // Commits to a node; Returns null if locked/cleared
        public NodeVisit Enter(NodeDefinition node, string instanceId, out string reason)
        {
            if (string.IsNullOrEmpty(instanceId) && node != null) instanceId = node.Id;
            if (InNode && CurrentVisit.InstanceId == instanceId)
            {
                reason = null;
                return CurrentVisit;
            }
            if (!CanEnter(node, instanceId, out reason)) return null;

            var visit = new NodeVisit(node, instanceId);
            visits[instanceId] = visit;
            CurrentVisit = visit;
            NodeEntered?.Invoke(visit);
            return visit;
        }

        // Choices
        public bool CanTakeChoice(int index, out string reason)
        {
            reason = null;
            if (!InNode) { reason = "Not inside a node"; return false; }
            NodeVisit visit = CurrentVisit;
            NodeDefinition node = visit.Definition;
            if (node.choices == null || index < 0 || index >= node.choices.Count)
            {
                reason = "No such choice";
                return false;
            }
            NodeChoice choice = node.choices[index];
            if (visit.HasTaken(index) && !choice.repeatable) { reason = "Already taken"; return false; }
            if (!visit.HasTaken(index) && node.maxChoices > 0 && visit.ChoicesTaken >= node.maxChoices)
            {
                reason = "No more choices here";
                return false;
            }
            if (!RunRequirement.AllMet(choice.requirements, this, out reason)) return false;
            if (Progress.Gold < choice.goldCost) { reason = $"Costs {choice.goldCost} Gold"; return false; }
            return true;
        }

        public bool TryTakeChoice(int index, out string reason, List<GrantedReward> granted = null)
        {
            if (!CanTakeChoice(index, out reason)) return false;
            NodeVisit visit = CurrentVisit;
            NodeChoice choice = visit.Definition.choices[index];
            if (!Progress.TrySpendGold(choice.goldCost)) { reason = $"Costs {choice.goldCost} Gold"; return false; }
            visit.MarkTaken(index);
            string source = $"{visit.Definition.displayName} / {choice.label}";
            GrantAll(choice.rewards, source, granted);
            return true;
        }

        // Completion
        // Grants the node completion rewards only once
        public List<GrantedReward> CompleteCurrent()
        {
            var granted = new List<GrantedReward>();
            if (!InNode) return granted;
            NodeVisit visit = CurrentVisit;
            if (!Progress.MarkNodeCompleted(visit.InstanceId))
            {
                visit.State = VisitState.Completed;
                return granted;
            }
            GrantAll(visit.Definition.completionRewards, visit.Definition.displayName, granted);
            visit.State = VisitState.Completed;
            NodeCompleted?.Invoke(visit);
            return granted;
        }

        // for rooms that finish on their own 
        public List<GrantedReward> CompleteNode(NodeDefinition node, string instanceId)
        {
            if (Enter(node, instanceId, out _) == null) return new List<GrantedReward>();
            return CompleteCurrent();
        }

        // Enemy kills give XP/gold; mastery pts never come from regular enemy
        public void GrantKillRewards(int xp, int goldMin, int goldMax, string source)
        {
            Grant(new RunReward(RewardType.Experience, xp), source, null);
            Grant(new RunReward(RewardType.Gold, goldMin, goldMax), source, null);
        }

        // Build spending
        public bool TryAllocateAttribute(AttributeType type, int points = 1)
        {
            if (!Progress.TryAllocate(type, points)) return false;
            AttributesChanged?.Invoke();
            return true;
        }

        public bool TryInvestMastery(string family, int points = 1) => Progress.TryInvestMastery(family, points);

        public int RollRange(int min, int max) => new RunReward(RewardType.Gold, min, max).Roll(Rng);

        // Rewards
        private void GrantAll(IList<RunReward> rewards, string source, List<GrantedReward> granted)
        {
            if (rewards == null) return;
            for (int i = 0; i < rewards.Count; i++) Grant(rewards[i], source, granted);
        }

        private void Grant(RunReward reward, string source, List<GrantedReward> granted)
        {
            int amount = reward.Roll(Rng);
            switch (reward.type)
            {
                case RewardType.Gold:
                    if (amount <= 0) return;
                    Progress.AddGold(amount);
                    break;
                case RewardType.Experience:
                    if (amount <= 0) return;
                    Progress.AddXp(amount);
                    break;
                case RewardType.AttributePoints:
                    if (amount <= 0) return;
                    Progress.AddAttributePoints(amount);
                    break;
                case RewardType.MasteryPoints:
                    if (amount <= 0) return;
                    Progress.AddMastery(amount);
                    break;
                case RewardType.Heal:
                    if (amount <= 0) return;
                    HealRequested?.Invoke(amount);
                    break;
                case RewardType.AttributeRespec:
                    amount = Progress.RefundAllAttributes();
                    AttributesChanged?.Invoke();
                    break;
                case RewardType.MasteryRespec:
                    amount = Progress.RefundAllMastery();
                    break;
            }
            var result = new GrantedReward(reward.type, amount, source);
            granted?.Add(result);
            RewardGranted?.Invoke(result);
        }
    }
}
