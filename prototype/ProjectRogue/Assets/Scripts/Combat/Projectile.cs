using UnityEngine;

// Arrow / magic bolt. Put this on a small prefab that also has a Rigidbody2D and a Collider2D.
// It deals damage through CombatResolver on the first valid target it touches, then disappears.
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    GameObject attacker;
    AbilityData ability;
    float baseDamage;
    LayerMask hitMask;
    bool used;

    public void Launch(GameObject attacker, AbilityData ability, float baseDamage, LayerMask hitMask, Vector2 direction)
    {
        this.attacker = attacker;
        this.ability = ability;
        this.baseDamage = baseDamage;
        this.hitMask = hitMask;

        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = direction.normalized * ability.projectileSpeed; // Unity 6 name (was velocity)

        GetComponent<Collider2D>().isTrigger = true;

        // Face the direction of travel (sprite assumed to point right).
        transform.right = direction;

        Destroy(gameObject, ability.projectileLifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used || ability == null) return;
        if (attacker != null && other.transform.IsChildOf(attacker.transform)) return;
        if ((hitMask.value & (1 << other.gameObject.layer)) == 0) return;

        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target == null) return;

        used = true;
        CombatResolver.Apply(attacker, ability, baseDamage, target);
        Destroy(gameObject);
    }
}
