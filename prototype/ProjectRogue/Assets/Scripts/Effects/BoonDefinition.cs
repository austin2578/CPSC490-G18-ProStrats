using System;
using System.Collections.Generic;
using UnityEngine;

public enum TriggerType { Hit, Crit, Kill, Dodge, StatusApplied, DamageTaken, DamageDealt }

[Serializable]
public class BoonTrigger
{
    public TriggerType trigger;
    public EffectDefinition effect;
    [Range(0f, 1f)] public float chance = 1f;

    [Header("Optional conditions")]
    public bool filterDamageType;
    public DamageType damageTypeFilter;
    public StatusDefinition requiresTargetStatus;

    [Header("Rule-changing: let this effect do more than the default")]
    public ProcPermissions grantToChild = ProcPermissions.None;
    public float effectiveness = 1f;
}

// wildfire: hits from this origin get extra perms (ex) burn ticks can trigger upon hit at 50% chance).
[Serializable]
public class ContextOverride
{
    public OriginType origin = OriginType.StatusTick;
    public ProcPermissions grant = ProcPermissions.TriggerOnHit;
    public float effectiveness = 0.5f;
}


[Serializable]
public class ConditionalDamage
{
    public StatusDefinition targetHasStatus;
    public string requiredTag;                 // tags come from AbilityData.cs tags
    public float bonus = 0.25f;                // 0.25 = +25% bonus
}

[CreateAssetMenu(menuName = "Rogue/Boon Definition")]
public class BoonDefinition : ScriptableObject
{
    public string Id => name;
    public string displayName;
    [TextArea] public string description;
    public List<StatModifierData> modifiers = new List<StatModifierData>();
    public List<BoonTrigger> triggers = new List<BoonTrigger>();
    public List<ContextOverride> contextOverrides = new List<ContextOverride>();
    public List<ConditionalDamage> conditionalDamage = new List<ConditionalDamage>();
}