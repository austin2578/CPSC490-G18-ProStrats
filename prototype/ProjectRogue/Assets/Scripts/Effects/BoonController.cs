using System;
using System.Collections.Generic;
using UnityEngine;

// apply on player (next to CombatStats); applies boon stat modifiers and routes combat events to triggers
[RequireComponent(typeof(CombatStats))]
public class BoonController : MonoBehaviour
{
    [SerializeField] bool logChains = false; // prints effect chain for every triggered effect
    [SerializeField] List<BoonDefinition> startingBoons = new List<BoonDefinition>();

    CombatStats stats;
    readonly List<BoonDefinition> boons = new List<BoonDefinition>();

    void Awake()
    {
        stats = GetComponent<CombatStats>();
        foreach (var b in startingBoons) Add(b);
    }

    void OnEnable()
    {
        CombatEvents.PreDamage += OnPreDamage;
        CombatEvents.Hit += OnHit;
        CombatEvents.Crit += OnCrit;
        CombatEvents.Kill += OnKill;
        CombatEvents.Dodged += OnDodged;
        CombatEvents.StatusApplied += OnStatusApplied;
        CombatResolver.DamageResolved += OnDamageResolved;
    }

    void OnDisable()
    {
        CombatEvents.PreDamage -= OnPreDamage;
        CombatEvents.Hit -= OnHit;
        CombatEvents.Crit -= OnCrit;
        CombatEvents.Kill -= OnKill;
        CombatEvents.Dodged -= OnDodged;
        CombatEvents.StatusApplied -= OnStatusApplied;
        CombatResolver.DamageResolved -= OnDamageResolved;
    }

    public void Add(BoonDefinition b)
    {
        if (b == null) return;
        boons.Add(b);
        foreach (var m in b.modifiers) stats.AddModifier(m.Create(b));
    }

    public void Remove(BoonDefinition b)
    {
        boons.Remove(b);
        stats.RemoveFromSource(b);
    }

    // event handlers
    void OnPreDamage(ref DamageInfo info)
    {
        if (info.source != gameObject) return;

        foreach (var b in boons)
        {
            foreach (var o in b.contextOverrides)
                if (o.origin == info.context.Origin)
                {
                    info.context.Permissions |= o.grant;
                    info.context.Effectiveness *= o.effectiveness;
                }

            foreach (var c in b.conditionalDamage)
            {
                if (!string.IsNullOrEmpty(c.requiredTag) && (info.tags == null || Array.IndexOf(info.tags, c.requiredTag) < 0)) continue;
                if (c.targetHasStatus != null)
                {
                    var sc = info.target != null ? info.target.GetComponent<StatusController>() : null;
                    if (sc == null || !sc.Has(c.targetHasStatus)) continue;
                }
                info.amount *= 1f + c.bonus;
            }
        }
    }

    void OnHit(DamageInfo i) => Fire(TriggerType.Hit, i.source, i.target, i.context, i);
    void OnCrit(DamageInfo i) => Fire(TriggerType.Crit, i.source, i.target, i.context, i);
    void OnKill(DamageInfo i) => Fire(TriggerType.Kill, i.source, i.target, i.context, i);
    void OnDodged(GameObject who, EffectContext c) => Fire(TriggerType.Dodge, who, null, c, null);
    void OnStatusApplied(GameObject target, StatusInstance s, EffectContext c) => Fire(TriggerType.StatusApplied, c.Source, target, c, null);

    void OnDamageResolved(DamageInfo i, IDamageable t)
    {
        Fire(TriggerType.DamageDealt, i.source, i.target, i.context, i);
        Fire(TriggerType.DamageTaken, i.target, i.source, i.context, i);
    }

    void Fire(TriggerType type, GameObject who, GameObject other, EffectContext ctx, DamageInfo? info)
    {
        if (who != gameObject || ctx == null) return;
        if (!ctx.CanChain) return; // proc depth guard

        bool direct = ctx.Origin == OriginType.PlayerAction;
        bool needsSecondary = type != TriggerType.Hit && type != TriggerType.Crit;
        if (needsSecondary && !direct && !ctx.Permissions.HasFlag(ProcPermissions.TriggerSecondary)) return;

        foreach (var b in boons)
            foreach (var t in b.triggers)
            {
                if (t.trigger != type || t.effect == null) continue;
                if (t.chance < 1f && UnityEngine.Random.value > t.chance) continue;
                if (t.requiresTargetStatus != null)
                {
                    var sc = other != null ? other.GetComponent<StatusController>() : null;
                    if (sc == null || !sc.Has(t.requiresTargetStatus)) continue;
                }

                float value = 0f;
                if (info.HasValue)
                {
                    if (t.filterDamageType && info.Value.type != t.damageTypeFilter) continue;
                    value = info.Value.amount;
                }
                else if (t.filterDamageType) continue;

                var child = ctx.CreateChild(b.Id, OriginType.BoonEffect, t.grantToChild, t.effectiveness);
                if (logChains) Debug.Log($"[Boon] {child.Chain} (depth {child.ProcDepth})");
                t.effect.Execute(child, gameObject, other, value);
            }
    }
}