using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectRogue.Run.Editor
{
    public static class RunSetup
    {
        public const string NodeFolder = "Assets/ScriptableObjects/RunNodes";
        public const string ReportPath = "Logs/run-rewards-result.txt";


        [MenuItem("Project Rogue/Run/Create Sample Node Assets")]
        public static List<NodeDefinition> CreateSampleNodeAssets()
        {
            EnsureFolder(NodeFolder);
            var result = new List<NodeDefinition>();
            foreach (NodeDefinition node in RunNodes.Create())
            {
                string path = $"{NodeFolder}/{node.id}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<NodeDefinition>(path);
                if (existing != null)
                {
                    // Keep designer edits; only create what is missing.
                    Object.DestroyImmediate(node);
                    result.Add(existing);
                    continue;
                }
                AssetDatabase.CreateAsset(node, path);
                result.Add(node);
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"Run nodes ready in {NodeFolder} ({result.Count} assets).");
            return result;
        }


        [MenuItem("Project Rogue/Run/Add Run Systems To Open Scene")]
        public static void AddRunSystemsToOpenScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode first.");
            if (Object.FindFirstObjectByType<RunManager>() != null)
            {
                Debug.Log("This scene already has a RunManager.");
                return;
            }
            List<NodeDefinition> nodes = CreateSampleNodeAssets();
            var combat = Object.FindFirstObjectByType<PlayerCombat>();
            AttributeSet attributes = combat != null ? combat.GetComponent<AttributeSet>() : Object.FindFirstObjectByType<AttributeSet>();
            if (attributes == null)
                Debug.LogWarning("No AttributeSet found on a player; add one so Attribute Points change combat scaling.");
            GameObject manager = CreateRunManager(attributes, nodes);
            Undo.RegisterCreatedObjectUndo(manager, "Add Run Systems");
            EditorSceneManager.MarkSceneDirty(manager.scene);
            Selection.activeGameObject = manager;
        }

        private static GameObject CreateRunManager(AttributeSet attributes, List<NodeDefinition> nodes)
        {
            var go = new GameObject("Run Manager");
            var manager = go.AddComponent<RunManager>();
            var panel = go.AddComponent<RunDebugPanel>();

            var m = new SerializedObject(manager);
            m.FindProperty("playerAttributes").objectReferenceValue = attributes;
            m.FindProperty("playerHealth").objectReferenceValue = attributes != null ? attributes.GetComponent<Health>() : null;
            m.ApplyModifiedPropertiesWithoutUndo();

            var p = new SerializedObject(panel);
            p.FindProperty("run").objectReferenceValue = manager;
            SerializedProperty pool = p.FindProperty("nodePool");
            pool.arraySize = nodes.Count;
            for (int i = 0; i < nodes.Count; i++) pool.GetArrayElementAtIndex(i).objectReferenceValue = nodes[i];
            p.ApplyModifiedPropertiesWithoutUndo();
            return go;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        // Test every acceptance criterion against sample nodes
        [MenuItem("Project Rogue/Run/Verify Run Rewards")]
        public static void Verify()
        {
            List<NodeDefinition> nodes = RunNodes.Create();
            var lines = new List<string>();
            try
            {
                NodeDefinition N(string id) => nodes.First(n => n.id == id);

                // 7 distinct node/reward types
                int nodeTypes = nodes.Select(n => n.nodeType).Distinct().Count();
                var rewardTypes = new HashSet<RewardType>();
                foreach (NodeDefinition n in nodes)
                {
                    foreach (RunReward r in n.completionRewards) rewardTypes.Add(r.type);
                    foreach (NodeChoice c in n.choices) foreach (RunReward r in c.rewards) rewardTypes.Add(r.type);
                }
                Require(nodeTypes >= 4, "Need at least 4 node types.");
                Require(rewardTypes.Count >= 4, "Need at least 4 reward types.");
                lines.Add($"node types: {nodeTypes}, reward types: {rewardTypes.Count}");

                int heals = 0, attributeEvents = 0, levelUps = 0;
                var s = new RunSession(new RunTuning(), 12345, t => 5);
                s.HealRequested += _ => heals++;
                s.AttributesChanged += () => attributeEvents++;
                s.Progress.LeveledUp += (_, __) => levelUps++;
                RunProgress p = s.Progress;

                // Gold + XP from completing a node
                Require(s.CompleteNode(N("combat_skirmish"), "c1").Count == 2, "Combat should grant 2 rewards.");
                Require(p.Gold >= 20 && p.Gold <= 30 && p.TotalXp == 40, "Combat gold/xp out of range.");
                int goldAfterCombat = p.Gold;
                Require(s.CompleteNode(N("combat_skirmish"), "c1").Count == 0 && p.Gold == goldAfterCombat,
                    "A completed node must not grant again.");
                Require(s.Enter(N("combat_skirmish"), "c1", out string why) == null && why == "Already cleared",
                    "A completed node must refuse re-entry.");
                lines.Add($"combat: +{goldAfterCombat} Gold +40 XP; replay grants nothing");

                // Route gates start locked
                Require(!s.CanEnter(N("shrine_trial"), "sh", out why) && why.Contains("Level 3"), "Shrine should need Level 3.");
                Require(!s.CanEnter(N("secret_arcane_vault"), "v", out why) && why.Contains("Mind 8"), "Vault should need Mind 8.");

                // Elite: Mastery + XP -> level up -> 3 attribute pts
                s.CompleteNode(N("elite_warden"), "e1");
                Require(p.UnspentMastery == 1, "Elite should grant 1 Mastery.");
                Require(p.Level == 2 && p.Xp == 40 && p.UnspentAttributePoints == 3 && levelUps == 1,
                    "140 XP should reach Level 2 with 40 carry-over and 3 Attribute Points.");
                lines.Add("elite: +1 Mastery, Level 2, +3 Attribute Points");

                // Spending attribute pts changes build and opens specific attribute route
                Require(s.TryAllocateAttribute(AttributeType.Mind, 3) && s.GetAttribute(AttributeType.Mind) == 8,
                    "Allocating 3 Mind should give Mind 8.");
                Require(!s.TryAllocateAttribute(AttributeType.Strength), "Cannot allocate without points.");
                Require(s.CanEnter(N("secret_arcane_vault"), "v", out _), "Mind 8 should open the vault.");
                lines.Add("attributes: +3 Mind -> Mind 8 -> Arcane Vault unlocked");

                // Shop: Gold spending, mastery required items, mastery points reset
                Require(s.Enter(N("shop_merchant"), "shop1", out _) != null, "Shop should open.");
                Require(!s.CanEnter(N("combat_skirmish"), "c2", out why) && why.StartsWith("Finish"),
                    "Cannot enter another node while inside one.");
                int before = p.Gold;
                if (before < 90)
                    Require(!s.TryTakeChoice(1, out why) && why.Contains("90") && p.Gold == before,
                        "Unaffordable item must fail without spending.");
                p.AddGold(300);
                before = p.Gold;
                Require(!s.CanTakeChoice(2, out why) && why.Contains("Sword Mastery 1"),
                    "Sword Drills should need Sword Mastery 1.");
                Require(s.TryInvestMastery(RunNodes.SwordFamily) && p.GetMasteryInvested("sword") == 1,
                    "Mastery should invest into Sword (case-insensitive).");
                Require(s.TryTakeChoice(2, out _) && p.Gold == before - 60 && p.UnspentMastery == 1,
                    "Investing Sword Mastery should unlock Sword Drills.");
                Require(s.TryTakeChoice(1, out _) && p.UnspentMastery == 2, "Mastery Tome purchase.");
                Require(!s.TryTakeChoice(1, out why) && why == "Already taken", "Tome is one per visit.");
                Require(s.TryTakeChoice(0, out _) && s.TryTakeChoice(0, out _) && heals == 2, "Draughts are repeatable.");
                Require(s.TryTakeChoice(3, out _) && p.UnspentMastery == 3 && p.GetMasteryInvested("Sword") == 0,
                    "Unlearn Mastery should refund invested Sword Mastery.");
                Require(p.Gold == before - 60 - 90 - 50 - 40, "Gold after purchases.");
                s.CompleteCurrent();
                Require(!p.TrySpendGold(p.Gold + 1) && p.Gold >= 0, "Gold must not go negative.");
                lines.Add("shop: Sword Mastery unlocked Drills; Tome, 2 Draughts, Mastery respec; overspend refused");

                // More XP -> level 3 -> shrine opens and grants mastery
                s.GrantKillRewards(150, 0, 0, "test kills");
                Require(p.Level == 3 && p.UnspentAttributePoints == 3, "Kill XP should reach Level 3.");
                Require(s.CompleteNode(N("shrine_trial"), "sh").Count == 2 && p.UnspentMastery == 5,
                    "Shrine should open at Level 3 and grant 2 Mastery.");
                lines.Add("kills: Level 3 -> Trial Shrine unlocked, +2 Mastery");

                // Split allocation (permanent upgrades and temporary upgrades during current run)
                s.TryAllocateAttribute(AttributeType.Strength, 2);
                s.TryAllocateAttribute(AttributeType.Vigor, 1);
                Require(p.AllocatedAttributePoints == 6 && p.UnspentAttributePoints == 0, "Split allocation.");
                s.Enter(N("rest_campfire"), "r1", out _);
                Require(s.TryTakeChoice(1, out _), "Rest respec.");
                Require(p.UnspentAttributePoints == 6 && p.AllocatedAttributePoints == 0 &&
                        s.GetAttribute(AttributeType.Mind) == 5 && s.GetAttribute(AttributeType.Strength) == 5,
                    "Respec must refund all run points and keep the class baseline.");
                Require(!s.TryTakeChoice(0, out why) && why == "No more choices here", "Rest allows one choice.");
                s.CompleteCurrent();
                Require(!s.CanEnter(N("secret_arcane_vault"), "v2", out _), "Respec should re-lock the Mind gate.");
                Require(attributeEvents >= 4, "AttributesChanged should fire on allocation and respec.");
                lines.Add("rest: respec refunded 6 points, baseline kept, vault re-locked");

                // Same seed + same actions = same rolls (replayable runs).
                Require(SeededGold(nodes, 777) == SeededGold(nodes, 777), "Seeded runs must be deterministic.");
                lines.Add("seeded rolls are deterministic");

                string report = "PASS: Goal 3 run rewards\n- " + string.Join("\n- ", lines);
                Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
                File.WriteAllText(ReportPath, report);
                Debug.Log(report);
            }
            finally
            {
                foreach (NodeDefinition n in nodes) Object.DestroyImmediate(n);
            }
        }

        private static int SeededGold(List<NodeDefinition> nodes, int seed)
        {
            var s = new RunSession(new RunTuning(), seed, t => 5);
            foreach (string id in new[] { "combat_skirmish", "elite_warden", "boss_act1" })
                s.CompleteNode(nodes.First(n => n.id == id), id);
            return s.Progress.Gold;
        }

        private static void Require(bool success, string message)
        {
            if (!success) throw new InvalidOperationException("FAIL: " + message);
        }
    }
}
