using System;
using UnityEngine;

public enum OriginType { PlayerAction, BoonEffect, StatusTick, StatusThreshold, Environment }

[Flags]
public enum ProcPermissions
{
    None = 0,
    TriggerOnHit = 1 << 0,      // may fire "On-Hit" boons
    CanCrit = 1 << 1,
    CanApplyStatus = 1 << 2,
    TriggerSecondary = 1 << 3,  // may fire Kill / StatusApplied / Dodge-type boons
    All = TriggerOnHit | CanCrit | CanApplyStatus | TriggerSecondary
}

// travel with every hit/effect. 
// answers: who started this, through which chain, how deep, and what is it allowed to trigger.
// effect source tracking + proc depth"
public class EffectContext
{
    public static int MaxProcDepth = 3;

    public GameObject Source;          // who started the chain (the player)
    public string SourceId;            // "Sword Slash", "Wildfire", "Burn", etc
    public OriginType Origin;
    public int ProcDepth;              // 0 = direct player action
    public ProcPermissions Permissions;
    public float Effectiveness = 1f;   // effectiveniss of secondary effect
    public EffectContext Parent;

    public bool CanChain => ProcDepth < MaxProcDepth;

    public string Chain => Parent == null ? SourceId : Parent.Chain + " > " + SourceId;

    public static EffectContext PlayerAction(GameObject source, string sourceId) => new EffectContext
    {
        Source = source, SourceId = sourceId, Origin = OriginType.PlayerAction,
        ProcDepth = 0, Permissions = ProcPermissions.All
    };

    public static ProcPermissions DefaultPermissions(OriginType o)
    {
        switch (o)
        {
            case OriginType.PlayerAction: return ProcPermissions.All;
            case OriginType.BoonEffect: return ProcPermissions.CanApplyStatus; // one explosion can't make more explosions
            default: return ProcPermissions.None;                              // DoT ticks: damage only
        }
    }

    public EffectContext CreateChild(string sourceId, OriginType origin,
        ProcPermissions grant = ProcPermissions.None, float effectiveness = 1f)
    {
        return new EffectContext
        {
            Source = Source, SourceId = sourceId, Origin = origin,
            ProcDepth = ProcDepth + 1,
            Permissions = DefaultPermissions(origin) | grant,
            Effectiveness = Effectiveness * effectiveness,
            Parent = this
        };
    }

    public EffectContext Clone() => (EffectContext)MemberwiseClone();
}