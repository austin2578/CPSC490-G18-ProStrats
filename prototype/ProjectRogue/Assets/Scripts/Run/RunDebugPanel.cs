using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectRogue.Run
{
    //Temporary debug panel, cann be hidden
    // Until map exists, "branch" of nodes/doors to pick from, shows lock reasons, and
    // lets you make choices, complete nodes, and spend attribute/mastery pts
    public sealed class RunDebugPanel : MonoBehaviour
    {
        [SerializeField] private RunManager run;
        [SerializeField] private List<NodeDefinition> nodePool = new List<NodeDefinition>();
        [SerializeField, Min(1)] private int branchWidth = 3;
        [SerializeField, Min(1), Tooltip("Completed nodes before the Boss is the only option.")]
        private int nodesBeforeBoss = 6;
        [SerializeField] private string[] weaponFamilies = { "Sword", "Axe", "Bow", "Wand" };
        [SerializeField] private bool visible = true;

        private static readonly AttributeType[] AllAttributes =
            (AttributeType[])Enum.GetValues(typeof(AttributeType));
        private readonly List<NodeDefinition> branch = new List<NodeDefinition>();
        private readonly List<string> log = new List<string>();
        private RunSession hooked;
        private System.Random routeRng;
        private int depth;
        private int familyIndex;
        private bool actCleared;
        private string message;
        private Vector2 scroll;

        public IReadOnlyList<NodeDefinition> Branch => branch;
        public RunSession Session => hooked;
        public bool ActCleared => actCleared;
        public event Action BranchChanged;

        public string InstanceIdFor(NodeDefinition node) => node == null ? null : $"{node.Id}@{depth}";

        private void Start()
        {
            if (run == null) run = RunManager.Instance != null ? RunManager.Instance : FindFirstObjectByType<RunManager>();
            nodePool.RemoveAll(n => n == null);
            if (nodePool.Count == 0) nodePool.AddRange(RunNodes.Create());
        }

        private void Update()
        {
            if (run != null && run.Session != hooked) Hook(run.Session);
        }

        private void OnDestroy() => Hook(null);

        private void Hook(RunSession session)
        {
            if (hooked != null)
            {
                hooked.RewardGranted -= OnReward;
                hooked.Progress.LeveledUp -= OnLevelUp;
                hooked.NodeCompleted -= OnNodeCompleted;
            }
            hooked = session;
            log.Clear();
            depth = 0;
            actCleared = false;
            message = null;
            branch.Clear();
            if (session == null) return;
            session.RewardGranted += OnReward;
            session.Progress.LeveledUp += OnLevelUp;
            session.NodeCompleted += OnNodeCompleted;
            routeRng = new System.Random(unchecked(session.Seed * 31 + 7));
            RollBranch();
        }

        // ---- Route ---------------------------------------------------------
        public bool TryEnter(NodeDefinition node)
        {
            if (hooked == null || node == null || !branch.Contains(node)) return false;
            NodeVisit visit = hooked.Enter(node, InstanceIdFor(node), out string reason);
            message = visit != null ? $"Entered {node.displayName}" : reason;
            return visit != null;
        }

        private void OnNodeCompleted(NodeVisit visit)
        {
            if (visit.Definition.nodeType == NodeType.Boss)
            {
                actCleared = true;
                branch.Clear();
                BranchChanged?.Invoke();
                return;
            }
            depth++;
            RollBranch();
        }

        private void RollBranch()
        {
            branch.Clear();
            if (depth >= nodesBeforeBoss)
            {
                foreach (NodeDefinition n in nodePool)
                    if (n.nodeType == NodeType.Boss) { branch.Add(n); break; }
            }
            if (branch.Count == 0)
            {
                var candidates = new List<NodeDefinition>();
                foreach (NodeDefinition n in nodePool)
                    if (n.nodeType != NodeType.Boss) candidates.Add(n);
                for (int i = 0; i < branchWidth && candidates.Count > 0; i++)
                {
                    int pick = routeRng.Next(candidates.Count);
                    branch.Add(candidates[pick]);
                    candidates.RemoveAt(pick);
                }
            }
            BranchChanged?.Invoke();
        }

        private void OnReward(GrantedReward r) => Push(r.ToString());
        private void OnLevelUp(int level, int points) => Push($"LEVEL {level}: +{points} Attribute Points");

        private void Push(string line)
        {
            log.Insert(0, line);
            if (log.Count > 10) log.RemoveAt(log.Count - 1);
        }

        // ---- HUD -----------------------------------------------------------
        private void OnGUI()
        {
            const float width = 440f;
            if (GUI.Button(new Rect(Screen.width - 110, 10, 100, 24), visible ? "Hide Run" : "Show Run"))
                visible = !visible;
            if (!visible) return;

            var area = new Rect(Screen.width - width - 10, 40, width, Screen.height - 50);
            GUI.Box(area, GUIContent.none);
            GUILayout.BeginArea(new Rect(area.x + 8, area.y + 6, area.width - 16, area.height - 12));
            scroll = GUILayout.BeginScrollView(scroll);

            RunSession s = hooked;
            if (s == null)
            {
                GUILayout.Label("No RunManager / run in this scene.");
                GUILayout.EndScrollView();
                GUILayout.EndArea();
                return;
            }
            RunProgress p = s.Progress;

            GUILayout.Label($"<b>RUN</b>  seed {s.Seed}   depth {depth}   cleared {p.CompletedNodeCount}", Rich());
            GUILayout.Label($"Gold: {p.Gold}   (earned {p.TotalGoldEarned})");
            GUILayout.Label($"Level {p.Level}   XP {p.Xp}/{p.XpToNextLevel}   (total {p.TotalXp})");
            GUILayout.Label($"Attribute Points: {p.UnspentAttributePoints} unspent / {p.AllocatedAttributePoints} allocated");

            foreach (AttributeType t in AllAttributes)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{t}: {s.GetAttribute(t)}  (base {s.GetBaseline(t)} +{p.GetAllocated(t)})", GUILayout.Width(300));
                GUI.enabled = p.UnspentAttributePoints > 0;
                if (GUILayout.Button("+1")) { AttributeType attr = t; Defer(() => s.TryAllocateAttribute(attr)); }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(4);
            GUILayout.Label($"Mastery: {p.UnspentMastery} unspent / {p.TotalMasteryEarned} earned");
            if (weaponFamilies != null && weaponFamilies.Length > 0)
            {
                familyIndex = Mathf.Clamp(familyIndex, 0, weaponFamilies.Length - 1);
                string family = weaponFamilies[familyIndex];
                GUILayout.BeginHorizontal();
                if (GUILayout.Button($"Family: {family}", GUILayout.Width(170)))
                    familyIndex = (familyIndex + 1) % weaponFamilies.Length;
                GUI.enabled = p.UnspentMastery > 0;
                if (GUILayout.Button($"Invest 1 (now {p.GetMasteryInvested(family)})")) Defer(() => s.TryInvestMastery(family));
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
            if (p.MasteryByFamily.Count > 0)
            {
                var parts = new List<string>();
                foreach (var kv in p.MasteryByFamily) parts.Add($"{kv.Key} {kv.Value}");
                GUILayout.Label("Invested: " + string.Join(", ", parts));
            }

            GUILayout.Space(8);
            if (s.InNode) DrawCurrentNode(s);
            else DrawBranch(s);

            if (!string.IsNullOrEmpty(message)) GUILayout.Label($"<i>{message}</i>", Rich());

            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Simulate kill (+15 XP, 3-8 Gold)")) Defer(() => s.GrantKillRewards(15, 3, 8, "Debug kill"));
            if (GUILayout.Button("New Run") && run != null) Defer(run.StartNewRun);
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("<b>Recent rewards</b>", Rich());
            foreach (string line in log) GUILayout.Label(line);

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            // Apply clicks after the whole HUD is drawn so Layout and input passes match.
            if (pending != null)
            {
                Action action = pending;
                pending = null;
                action();
            }
        }

        private Action pending;
        private void Defer(Action action) => pending = action;

        private void DrawBranch(RunSession s)
        {
            if (actCleared)
            {
                GUILayout.Label("<b>Act cleared!</b> Press New Run to start again.", Rich());
                return;
            }
            GUILayout.Label("<b>Choose the next node</b> (press E at a door, or click Enter)", Rich());
            foreach (NodeDefinition node in branch)
            {
                bool open = s.CanEnter(node, InstanceIdFor(node), out string reason);
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label($"<b>{node.displayName}</b>  [{node.nodeType}]", Rich());
                GUILayout.Label($"Rewards: {RewardText.Join(node.completionRewards)}" +
                                (node.choices.Count > 0 ? $"   Choices: {node.choices.Count}" : ""));
                if (node.entryRequirements.Count > 0)
                    GUILayout.Label($"Needs: {RunRequirement.Join(node.entryRequirements)}");
                GUI.enabled = open;
                if (GUILayout.Button(open ? "Enter" : $"Locked - {reason}")) { NodeDefinition target = node; Defer(() => TryEnter(target)); }
                GUI.enabled = true;
                GUILayout.EndVertical();
            }
        }

        private void DrawCurrentNode(RunSession s)
        {
            NodeVisit visit = s.CurrentVisit;
            NodeDefinition node = visit.Definition;
            GUILayout.Label($"<b>IN NODE: {node.displayName}</b>  [{node.nodeType}]", Rich());
            if (!string.IsNullOrEmpty(node.description)) GUILayout.Label(node.description);

            for (int i = 0; i < node.choices.Count; i++)
            {
                NodeChoice c = node.choices[i];
                bool ok = s.CanTakeChoice(i, out string reason);
                string cost = c.goldCost > 0 ? $" ({c.goldCost}g)" : "";
                GUI.enabled = ok;
                if (GUILayout.Button($"{c.label}{cost}: {RewardText.Join(c.rewards)}" + (ok ? "" : $"  - {reason}")))
                {
                    int index = i;
                    Defer(() => message = s.TryTakeChoice(index, out string fail) ? $"Took {c.label}" : fail);
                }
                GUI.enabled = true;
            }
            if (GUILayout.Button($"Complete node  ->  {RewardText.Join(node.completionRewards)}"))
            {
                Defer(() =>
                {
                    s.CompleteCurrent();
                    message = $"Completed {node.displayName}";
                });
            }
        }

        private static GUIStyle rich;
        private static GUIStyle Rich()
        {
            if (rich == null) rich = new GUIStyle(GUI.skin.label) { richText = true };
            return rich;
        }
    }
}
