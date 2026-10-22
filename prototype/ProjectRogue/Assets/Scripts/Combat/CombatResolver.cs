using System;
using UnityEngine;

// The single place every hit goes through. Weapon scaling happens before this
// (baseDamage comes in already scaled); boons will subscribe to DamageResolved
// or get a modifier hook added here for Objective 1.4.
public static class CombatResolver
{
    public static event Action<DamageInfo, IDamageable> DamageResolved;

    public static void Apply(GameObject attacker, AbilityData ability, float baseDamage, IDamageable target)
    {
        var info = new DamageInfo
        {
            source = attacker,
            amount = baseDamage * ability.damageMultiplier,
            type = ability.damageType,
            tags = ability.tags
        };

        target.TakeDamage(info);
        DamageResolved?.Invoke(info, target);
    }
}
