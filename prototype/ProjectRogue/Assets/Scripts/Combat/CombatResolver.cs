using System;
using UnityEngine;

// The single place every hit goes through (basic attacks and active abilities alike).
// Boons will subscribe to DamageResolved or get a modifier hook added here for Objective 1.4.
public static class CombatResolver
{
    public static event Action<DamageInfo, IDamageable> DamageResolved;


    public static float ComputeDamage(AbilityData ability, float baseDamage, AttributeSet attackerAttributes)
    {
        float scaling = 0f;
        if (attackerAttributes != null)
            scaling = attackerAttributes.Get(ability.scalingAttribute) * ability.scalingPerPoint;

        return (baseDamage + scaling) * ability.damageMultiplier;
    }

    // 'damage' is the final amount from ComputeDamage.
    public static void Apply(GameObject attacker, AbilityData ability, float damage, IDamageable target)
    {
        var ctx = EffectContext.PlayerAction(attacker, ability.displayName);
        Resolve(attacker, ability.damageType, damage, ability.tags, target, ctx);
    }

    // autpmatically called when boons and status ticks need to update.
    public static DamageInfo Resolve(GameObject attacker, DamageType type, float damage, string[] tags,
        IDamageable target, EffectContext ctx)
    {
        var targetComp = target as Component;
        GameObject targetGo = targetComp != null ? targetComp.gameObject : null;
        CombatStats atk = attacker != null ? attacker.GetComponent<CombatStats>() : null;
        CombatStats tgt = targetGo != null ? targetGo.GetComponent<CombatStats>() : null;

        var info = new DamageInfo
        {
            source = attacker, target = targetGo, amount = damage, type = type, tags = tags,
            context = ctx.Clone() // boons may adjust this hit's permissions without leaking into its siblings
        };

        if (IsDead(target)) return info;

        if (tgt != null && tgt.IsInvulnerable) // dodges i-frames
        {
            info.amount = 0f;
            CombatEvents.RaiseDodged(targetGo, info.context);
            return info;
        }

        CombatEvents.RaisePreDamage(ref info); // boons modify hit
        var c = info.context;

        if (atk != null && c.Permissions.HasFlag(ProcPermissions.CanCrit))
            info.isCrit = UnityEngine.Random.value < atk.Get(StatId.CritChance);

        float amt = info.amount;
        if (atk != null)
        {
            amt *= Mathf.Max(0f, 1f + atk.Get(StatId.DamageBonusAll) + atk.Get(StatId.DamageBonus(type)));
            if (info.isCrit) amt *= Mathf.Max(1f, atk.Get(StatId.CritMultiplier));
        }
        if (tgt != null)
        {
            amt *= Mathf.Max(0f, 1f + tgt.Get(StatId.DamageTaken(type)));
            amt *= 1f - Mathf.Clamp(tgt.Get(StatId.Resistance(type)), -1f, 0.9f);
        }

        info.amount = amt;

        target.TakeDamage(info);
        bool killed = IsDead(target);

        DamageResolved?.Invoke(info, target);
        if (c.Permissions.HasFlag(ProcPermissions.TriggerOnHit)) CombatEvents.RaiseHit(info);
        if (info.isCrit) CombatEvents.RaiseCrit(info);
        if (killed) CombatEvents.RaiseKill(info);
        return info;
    }

    // IDamageable.cs has no IsDead; check Health.cs directly
    public static bool IsDead(IDamageable target) => target is Health h && h.IsDead;
}
