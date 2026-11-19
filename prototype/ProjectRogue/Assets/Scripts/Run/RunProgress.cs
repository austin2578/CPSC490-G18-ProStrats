using System;
using System.Collections.Generic;

namespace ProjectRogue.Run
{
    // Tuning numbers are placeholders for playtesting, not final
    [Serializable]
    public class RunTuning
    {
        public int startingGold = 0;
        public int attributePointsPerLevel = 3;   // LOCKED
        public int baseXpToLevel = 100;           // XP needed for level up from baseline
        public int xpGrowthPerLevel = 50;         // each level after needs this much more XP
        public int maxLevel = 30;
    }

    // Everything the run tracks and gets thrown away after run ends
    public sealed class RunProgress
    {
        private static readonly int AttributeCount = Enum.GetValues(typeof(AttributeType)).Length;

        private readonly RunTuning tuning;
        private readonly int[] allocated = new int[AttributeCount];
        private readonly Dictionary<string, int> masteryInvested =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> completedNodes = new HashSet<string>();

        public event Action Changed;
        public event Action<int, int> LeveledUp; // new level, attribute pts granted

        public RunProgress(RunTuning tuning)
        {
            this.tuning = tuning ?? new RunTuning();
            Level = 1;
            Gold = Math.Max(0, this.tuning.startingGold);
        }

        // Gold
        public int Gold { get; private set; }
        public int TotalGoldEarned { get; private set; }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            Gold += amount;
            TotalGoldEarned += amount;
            Changed?.Invoke();
        }

        public bool TrySpendGold(int amount)
        {
            if (amount < 0 || Gold < amount) return false;
            if (amount == 0) return true;
            Gold -= amount;
            Changed?.Invoke();
            return true;
        }

        // XP / Level 
        public int Level { get; private set; }
        public int Xp { get; private set; }          // progress inside the current level
        public int TotalXp { get; private set; }
        public int XpToNextLevel => XpRequiredFor(Level);
        public bool IsMaxLevel => Level >= tuning.maxLevel;

        public int XpRequiredFor(int level) =>
            Math.Max(1, tuning.baseXpToLevel + tuning.xpGrowthPerLevel * (level - 1));

        public void AddXp(int amount)
        {
            if (amount <= 0) return;
            TotalXp += amount;
            Xp += amount;
            while (!IsMaxLevel && Xp >= XpToNextLevel)
            {
                Xp -= XpToNextLevel;
                Level++;
                int points = Math.Max(0, tuning.attributePointsPerLevel);
                UnspentAttributePoints += points;
                LeveledUp?.Invoke(Level, points);
            }
            Changed?.Invoke();
        }

        // Attribute Points 
        // Points build up; collected in UnspentAttributePoints until used by player
        public int UnspentAttributePoints { get; private set; }
        public int AllocatedAttributePoints
        {
            get { int sum = 0; for (int i = 0; i < allocated.Length; i++) sum += allocated[i]; return sum; }
        }

        public int GetAllocated(AttributeType type) => allocated[(int)type];

        public void AddAttributePoints(int amount)
        {
            if (amount <= 0) return;
            UnspentAttributePoints += amount;
            Changed?.Invoke();
        }

        public bool TryAllocate(AttributeType type, int points = 1)
        {
            if (points <= 0 || UnspentAttributePoints < points) return false;
            UnspentAttributePoints -= points;
            allocated[(int)type] += points;
            Changed?.Invoke();
            return true;
        }

        // Full respec: every run-earned point comes back; class baseline is not touched
        public int RefundAllAttributes()
        {
            int refunded = AllocatedAttributePoints;
            Array.Clear(allocated, 0, allocated.Length);
            UnspentAttributePoints += refunded;
            Changed?.Invoke();
            return refunded;
        }

        // Mastery
        // Earned from run, spent into one weapon family tree, stays there until reset
        public int UnspentMastery { get; private set; }
        public int TotalMasteryEarned { get; private set; }
        public IReadOnlyDictionary<string, int> MasteryByFamily => masteryInvested;

        public void AddMastery(int amount)
        {
            if (amount <= 0) return;
            UnspentMastery += amount;
            TotalMasteryEarned += amount;
            Changed?.Invoke();
        }

        public int GetMasteryInvested(string family)
        {
            if (string.IsNullOrEmpty(family)) return 0;
            return masteryInvested.TryGetValue(family, out int v) ? v : 0;
        }

        public bool TryInvestMastery(string family, int points = 1)
        {
            if (string.IsNullOrEmpty(family) || points <= 0 || UnspentMastery < points) return false;
            UnspentMastery -= points;
            masteryInvested[family] = GetMasteryInvested(family) + points;
            Changed?.Invoke();
            return true;
        }

        public int RefundMastery(string family)
        {
            int refunded = GetMasteryInvested(family);
            if (refunded == 0) return 0;
            masteryInvested.Remove(family);
            UnspentMastery += refunded;
            Changed?.Invoke();
            return refunded;
        }

        public int RefundAllMastery()
        {
            int refunded = 0;
            foreach (int v in masteryInvested.Values) refunded += v;
            masteryInvested.Clear();
            UnspentMastery += refunded;
            if (refunded > 0) Changed?.Invoke();
            return refunded;
        }

        // Node completion 
        // No farming completed encounters, so ids are remembered during a run
        public int CompletedNodeCount => completedNodes.Count;
        public bool IsNodeCompleted(string instanceId) => completedNodes.Contains(instanceId);
        public bool MarkNodeCompleted(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId) || !completedNodes.Add(instanceId)) return false;
            Changed?.Invoke();
            return true;
        }
    }
}
