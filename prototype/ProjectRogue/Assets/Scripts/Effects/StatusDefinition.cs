using System.Collections.Generic;
using UnityEngine;

// 1 asset/icon per status; Burn, Poison, Bleed, Chill, etc.
// shared status system for duration, stacking, removal, triggers
[CreateAssetMenu(menuName = "Rogue/Status Definition")]
public class StatusDefinition : ScriptableObject
{
    public string Id => name;
    public int maxStacks = 1;
    public float duration = 4f;

    [Header("Damage over time (leave array empty for none)")]
    public float tickInterval = 1f;
    public DamageType tickDamageType = DamageType.Fire;
    [Tooltip("Total tick damage at N stacks = element N-1. Poison example: 1,3,6,10,15")]
    public float[] tickDamageByStack;

    [Header("Modifiers while on the afflicted object")]
    [Tooltip("Poison: DamageBonus.All -0.2.  Bleed: DamageTaken.Physical +0.25.  Curse: Resistance.* negative.  Buffs: CritChance +0.2.")]
    public List<StatModifierData> holderModifiers = new List<StatModifierData>();

    [Header("Threshold payoff (Chill->Freeze, Judgment->Smite)")]
    public int thresholdStacks;
    public EffectDefinition thresholdEffect;
    public bool clearOnThreshold = true;
}

public class StatusInstance
{
    public StatusDefinition Def;
    public int Stacks;
    public float Remaining, TickTimer;
    public EffectContext SourceContext; // who applied it and through which chain
    public StatusInstance(StatusDefinition d) { Def = d; }
}