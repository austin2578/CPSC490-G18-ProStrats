using UnityEngine;

// Arrow / magic bolt.
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    GameObject attacker;
    AbilityData ability;
    float damage; // final, already scaled (CombatResolver.ComputeDamage)
    LayerMask hitMask;
    bool used;

    public void Launch(GameObject attacker, AbilityData ability, float damage, LayerMask hitMask, Vector2 direction)
    {
        this.attacker = attacker;
        this.ability = ability;
        this.damage = damage;
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
        CombatResolver.Apply(attacker, ability, damage, target);
        Destroy(gameObject);
    }
}
