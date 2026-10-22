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
        var info = new DamageInfo
        {
            source = attacker,
            amount = damage,
            type = ability.damageType,
            tags = ability.tags
        };

        target.TakeDamage(info);
        DamageResolved?.Invoke(info, target);
    }
}
