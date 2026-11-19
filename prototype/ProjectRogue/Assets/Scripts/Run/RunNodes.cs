using System.Collections.Generic;
using UnityEngine;

namespace ProjectRogue.Run
{
    // Prototype content: 7 different nodes (more to be added later) taken from the gameplaydesign doc
    // Numbers are placeholders for playtesting, not final
    public static class RunNodes
    {
        public const string SwordFamily = "Sword";

        public static List<NodeDefinition> Create()
        {
            var list = new List<NodeDefinition>();

            list.Add(Node("combat_skirmish", "Skirmish", NodeType.Combat,
                "Clear the room of enemies.",
                rewards: new[] { R(RewardType.Gold, 20, 30), R(RewardType.Experience, 40) }));

            list.Add(Node("elite_warden", "Warden (Elite)", NodeType.Elite,
                "A hard optional fight with superior rewards, including Mastery.",
                rewards: new[] { R(RewardType.Gold, 55, 75), R(RewardType.Experience, 100), R(RewardType.MasteryPoints, 1) }));

            list.Add(Node("shop_merchant", "Wandering Merchant", NodeType.Shop,
                "Spend Gold. Buy as many different items as you can afford.",
                choices: new[]
                {
                    Choice("Healing Draught", 25, true, null, R(RewardType.Heal, 40)),
                    Choice("Mastery Tome", 90, false, null, R(RewardType.MasteryPoints, 1)),
                    Choice("Sword Drills", 60, false, new[] { RunRequirement.Mastery(SwordFamily, 1) },
                        R(RewardType.MasteryPoints, 1)),
                    Choice("Unlearn Mastery", 40, false, null, R(RewardType.MasteryRespec, 1)),
                },
                maxChoices: 0));

            list.Add(Node("rest_campfire", "Campfire (Rest)", NodeType.Rest,
                "Recover, or take a free attribute respec.",
                choices: new[]
                {
                    Choice("Rest", 0, false, null, R(RewardType.Heal, 50)),
                    Choice("Reflect (attribute respec)", 0, false, null, R(RewardType.AttributeRespec, 1)),
                },
                maxChoices: 1));

            list.Add(Node("shrine_trial", "Trial Shrine", NodeType.Shrine,
                "A trial for seasoned runners.",
                entry: new[] { RunRequirement.Level(3) },
                rewards: new[] { R(RewardType.MasteryPoints, 2), R(RewardType.Experience, 50) }));

            list.Add(Node("secret_arcane_vault", "Arcane Vault (Secret)", NodeType.Secret,
                "A sealed door that answers to a trained Mind.",
                entry: new[] { RunRequirement.Attribute(AttributeType.Mind, 8) },
                rewards: new[] { R(RewardType.Gold, 90, 120), R(RewardType.Experience, 30) }));

            list.Add(Node("boss_act1", "Act 1 Boss", NodeType.Boss,
                "The Act climax.",
                rewards: new[] { R(RewardType.Gold, 120, 150), R(RewardType.Experience, 200), R(RewardType.MasteryPoints, 2) }));

            return list;
        }

        private static RunReward R(RewardType type, int min, int max = 0) => new RunReward(type, min, max);

        private static NodeChoice Choice(string label, int cost, bool repeatable,
            RunRequirement[] requirements, params RunReward[] rewards)
        {
            var c = new NodeChoice { label = label, goldCost = cost, repeatable = repeatable };
            if (requirements != null) c.requirements.AddRange(requirements);
            c.rewards.AddRange(rewards);
            return c;
        }

        private static NodeDefinition Node(string id, string displayName, NodeType type, string description,
            RunReward[] rewards = null, RunRequirement[] entry = null, NodeChoice[] choices = null, int maxChoices = 1)
        {
            var n = ScriptableObject.CreateInstance<NodeDefinition>();
            n.name = id;
            n.id = id;
            n.displayName = displayName;
            n.nodeType = type;
            n.description = description;
            n.maxChoices = maxChoices;
            if (rewards != null) n.completionRewards.AddRange(rewards);
            if (entry != null) n.entryRequirements.AddRange(entry);
            if (choices != null) n.choices.AddRange(choices);
            return n;
        }
    }
}
