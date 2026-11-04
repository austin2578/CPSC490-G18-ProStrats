using System;
using System.Collections.Generic;
using UnityEngine;

// Stat IDs are string-backed so new stats need no code changes.
// Attributes (Vigor/Strength/...) stay in AttributeSet (Objective 1.2); these are COMBAT stats.
// Convention: "bonus" stats (DamageBonus.*, CritChance) use Flat modifiers (0.2 = +20%).
[Serializable]
public struct StatId : IEquatable<StatId>
{
    [SerializeField] string name;
    public StatId(string name) { this.name = name; }
    public bool Equals(StatId other) => name == other.name;
    public override bool Equals(object obj) => obj is StatId o && Equals(o);
    public override int GetHashCode() => name == null ? 0 : name.GetHashCode();
    public override string ToString() => name;

    // Resource USAGE (cost modifiers). Max Health/Stamina/Mana come from attributes, not boons.
    public static readonly StatId ResourceCostMultiplier = new StatId("ResourceCostMultiplier");

    // Offense
    public static readonly StatId CritChance = new StatId("CritChance");
    public static readonly StatId CritMultiplier = new StatId("CritMultiplier");
    public static readonly StatId AttackSpeed = new StatId("AttackSpeed");
    public static readonly StatId DamageBonusAll = new StatId("DamageBonus.All");

    public static StatId DamageBonus(DamageType t) => new StatId("DamageBonus." + t);
    public static StatId DamageTaken(DamageType t) => new StatId("DamageTaken." + t);   // Bleed raises Physical
    public static StatId Resistance(DamageType t) => new StatId("Resistance." + t);     // Curse lowers elemental
}

public enum ModifierType { Flat, PercentAdd, PercentMult }

// single change to a stat + what granted it (boon, status applied, weapon, etc)
// everything from one source can be removed together (status expiry, weapon swap, etc)
public class StatModifier
{
    public readonly StatId Stat;
    public readonly ModifierType Type;
    public readonly float Value;
    public readonly object Source;
    public StatModifier(StatId stat, ModifierType type, float value, object source)
    { Stat = stat; Type = type; Value = value; Source = source; }
}

[Serializable]
public struct StatModifierData
{
    public StatId stat;
    public ModifierType type;
    public float value;
    public bool scalesWithStacks;

    public StatModifier Create(object source, float stacks = 1f)
        => new StatModifier(stat, type, scalesWithStacks ? value * stacks : value, source);
}

public class Stat
{
    readonly List<StatModifier> mods = new List<StatModifier>();

    public void Add(StatModifier m) => mods.Add(m);
    public void RemoveFromSource(object source) => mods.RemoveAll(m => m.Source == source);

    public float Evaluate(float baseValue)
    {
        float flat = 0f, add = 0f, mult = 1f;
        for (int i = 0; i < mods.Count; i++)
        {
            var m = mods[i];
            if (m.Type == ModifierType.Flat) flat += m.Value;
            else if (m.Type == ModifierType.PercentAdd) add += m.Value;
            else mult *= 1f + m.Value;
        }
        return (baseValue + flat) * (1f + add) * mult;
    }
}