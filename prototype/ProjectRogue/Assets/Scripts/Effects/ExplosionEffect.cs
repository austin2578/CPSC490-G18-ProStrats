using System.Collections.Generic;
using UnityEngine;

// example of one effect type
// to add a new kind of effect later (healing, projectile burst, etc), copy this file,
// rename class + file, and override Execute()

[CreateAssetMenu(menuName = "Rogue/Effects/Explosion")]
public class ExplosionEffect : EffectDefinition
{
    public float radius = 2f;
    public DamageType damageType = DamageType.Fire;
    public float flatDamage = 10f;
    [Range(0f, 2f)] public float percentOfTriggerValue = 0f;
    [Tooltip("Set to the enemy layer(s) so explosions never hurt the player.")]
    public LayerMask targetLayers = ~0;
    public string[] tags;

    // called by BoonController
    // ctx = source chain/proc depth/perms for explosion
    // owner = player tha owns the boon
    // target = enemy that triggered it
    // triggerValue = dmg of triggering hit
    public override void Execute(EffectContext ctx, GameObject owner, GameObject target, float triggerValue)
    {
        Vector2 center = target != null ? (Vector2)target.transform.position : (Vector2)owner.transform.position;
        float dmg = (flatDamage + triggerValue * percentOfTriggerValue) * ctx.Effectiveness;
        var seen = new HashSet<IDamageable>();
        foreach (var col in Physics2D.OverlapCircleAll(center, radius, targetLayers))
        {
            if (col.transform.IsChildOf(owner.transform)) continue;
            var d = col.GetComponentInParent<IDamageable>();
            if (d == null || !seen.Add(d)) continue;
            CombatResolver.Resolve(owner, damageType, dmg, tags, d, ctx);
        }
    }
}