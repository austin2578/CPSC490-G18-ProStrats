using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectRogue.Run
{
    // Encounter categories from the gameplay design doc 
    // The map/room layer decides where node appears; this only indicates what kind of encounter is in the node
    public enum NodeType
    {
        Combat, Elite, Boss, Shop, Mystery, Treasure, Puzzle, Traversal,
        Rest, Forge, Trainer, Shrine, Secret
    }

    // Possible reward/choice from completed node
    public enum RewardType
    {
        Gold,
        Experience,       // give XP; each level grants Attribute pts
        AttributePoints,  // rare events only give straight Attribue pts
        MasteryPoints,    // run-local, spent into a weapon family
        Heal,             // restores Health
        AttributeRespec,  // refunds run specific Attribute pts, keeps baseline
        MasteryRespec     // refunds Mastery pts
    }

    [Serializable]
    public struct RunReward
    {
        public RewardType type;
        [Min(0)] public int min;
        [Min(0), Tooltip("Inclusive upper bound. Leave at or below Min for a fixed amount.")]
        public int max;

        public RunReward(RewardType type, int min, int max = 0)
        {
            this.type = type;
            this.min = min;
            this.max = max;
        }

        public int Roll(System.Random rng)
        {
            if (max <= min || rng == null) return min;
            return rng.Next(min, max + 1);
        }

        public override string ToString() => RewardText.Describe(type, min, max);
    }

    // A reward after it has been rolled and applied.
    public readonly struct GrantedReward
    {
        public readonly RewardType type;
        public readonly int amount;
        public readonly string source;

        public GrantedReward(RewardType type, int amount, string source)
        {
            this.type = type;
            this.amount = amount;
            this.source = source;
        }

        public override string ToString() => $"{source}: {RewardText.Describe(type, amount, amount)}";
    }

    public static class RewardText
    {
        public static string Describe(RewardType type, int min, int max)
        {
            string n = max > min ? $"{min}-{max}" : min.ToString();
            switch (type)
            {
                case RewardType.Gold: return $"+{n} Gold";
                case RewardType.Experience: return $"+{n} XP";
                case RewardType.AttributePoints: return $"+{n} Attribute Pt";
                case RewardType.MasteryPoints: return $"+{n} Mastery";
                case RewardType.Heal: return $"Heal {n}";
                case RewardType.AttributeRespec: return "Attribute respec";
                case RewardType.MasteryRespec: return "Mastery respec";
                default: return type.ToString();
            }
        }

        public static string Join(IList<RunReward> rewards)
        {
            if (rewards == null || rewards.Count == 0) return "-";
            var parts = new string[rewards.Count];
            for (int i = 0; i < rewards.Count; i++) parts[i] = rewards[i].ToString();
            return string.Join(", ", parts);
        }
    }

    // overviewview of the run used to check node / choice requirements.
    public interface IRunStats
    {
        int Level { get; }
        int Gold { get; }
        int UnspentMastery { get; }
        int GetAttribute(AttributeType type);           // class baseline + run-allocated points
        int GetMasteryInvested(string weaponFamily);
    }

    public enum RequirementKind { MinLevel, MinGold, MinAttribute, MinMasteryInvested, MinUnspentMastery }

    // Entry requirement on a node/choice; Rewards earned earlier in the run are what open these (rewards feed back into route decisions)
    [Serializable]
    public struct RunRequirement
    {
        public RequirementKind kind;
        [Min(0)] public int value;
        [Tooltip("Used by MinAttribute only.")]
        public AttributeType attribute;
        [Tooltip("Used by MinMasteryInvested only (e.g. Sword, Bow, Wand).")]
        public string weaponFamily;

        public static RunRequirement Level(int level) =>
            new RunRequirement { kind = RequirementKind.MinLevel, value = level };
        public static RunRequirement GoldAtLeast(int gold) =>
            new RunRequirement { kind = RequirementKind.MinGold, value = gold };
        public static RunRequirement Attribute(AttributeType type, int value) =>
            new RunRequirement { kind = RequirementKind.MinAttribute, attribute = type, value = value };
        public static RunRequirement Mastery(string family, int value) =>
            new RunRequirement { kind = RequirementKind.MinMasteryInvested, weaponFamily = family, value = value };

        public bool IsMet(IRunStats stats)
        {
            if (stats == null) return false;
            switch (kind)
            {
                case RequirementKind.MinLevel: return stats.Level >= value;
                case RequirementKind.MinGold: return stats.Gold >= value;
                case RequirementKind.MinAttribute: return stats.GetAttribute(attribute) >= value;
                case RequirementKind.MinMasteryInvested: return stats.GetMasteryInvested(weaponFamily) >= value;
                case RequirementKind.MinUnspentMastery: return stats.UnspentMastery >= value;
                default: return true;
            }
        }

        public string Describe()
        {
            switch (kind)
            {
                case RequirementKind.MinLevel: return $"Level {value}";
                case RequirementKind.MinGold: return $"{value} Gold";
                case RequirementKind.MinAttribute: return $"{attribute} {value}";
                case RequirementKind.MinMasteryInvested: return $"{weaponFamily} Mastery {value}";
                case RequirementKind.MinUnspentMastery: return $"{value} unspent Mastery";
                default: return kind.ToString();
            }
        }

        // True when every requirement is met, otherwise states the unmet requirement
        public static bool AllMet(IList<RunRequirement> requirements, IRunStats stats, out string reason)
        {
            reason = null;
            if (requirements == null) return true;
            for (int i = 0; i < requirements.Count; i++)
            {
                if (requirements[i].IsMet(stats)) continue;
                reason = "Requires " + requirements[i].Describe();
                return false;
            }
            return true;
        }

        public static string Join(IList<RunRequirement> requirements)
        {
            if (requirements == null || requirements.Count == 0) return "";
            var parts = new string[requirements.Count];
            for (int i = 0; i < requirements.Count; i++) parts[i] = requirements[i].Describe();
            return string.Join(", ", parts);
        }
    }
}
