using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ResourcePool))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Abilities (for testing 2.2 without a weapon)")]
    public AbilityData basicAttack;
    public AbilityData ability1;

    [Header("Damage")]
    public float fallbackBaseDamage = 10f;
    public LayerMask hitMask = ~0;

    ResourcePool resources;
    AttributeSet attributes;
    IWeaponSource weapon; // optional until 1.3 (EquipmentManager) is added to the player
    readonly Dictionary<AbilityData, float> readyAt = new Dictionary<AbilityData, float>();

    float Facing => Mathf.Sign(transform.localScale.x); // assumes you flip the sprite via scale.x

    void Awake()
    {
        resources = GetComponent<ResourcePool>();
        attributes = GetComponent<AttributeSet>();
        weapon = GetComponent<IWeaponSource>();
    }

    void Update()
    {
        if (DebugInput.Pressed(DebugKey.Basic)) TryUse(GetBasicAttack());
        if (DebugInput.Pressed(DebugKey.Ability1)) TryUse(ability1);

        if (weapon != null)
        {
            if (DebugInput.Pressed(DebugKey.Skill1)) TryUse(weapon.GetSkill(0));
            if (DebugInput.Pressed(DebugKey.Skill2)) TryUse(weapon.GetSkill(1));
            if (DebugInput.Pressed(DebugKey.Skill3)) TryUse(weapon.GetSkill(2));
        }
    }

    AbilityData GetBasicAttack()
    {
        if (weapon != null && weapon.HasWeapon && weapon.BasicAttack != null)
            return weapon.BasicAttack;
        return basicAttack;
    }

    // Every attack, basic or skill, goes: cooldown check -> resource cost -> scaled damage -> hit.
    public bool TryUse(AbilityData ability)
    {
        if (ability == null) return false;
        if (readyAt.TryGetValue(ability, out float t) && Time.time < t) return false;
        if (!resources.TrySpend(ability.resource, ability.cost)) return false;

        readyAt[ability] = Time.time + ability.cooldown;
        Perform(ability);
        return true;
    }

    void Perform(AbilityData ability)
    {
        float baseDamage = (weapon != null && weapon.HasWeapon)
            ? weapon.GetBaseDamage(attributes)
            : fallbackBaseDamage;

        float damage = CombatResolver.ComputeDamage(ability, baseDamage, attributes);

        if (ability.shape == AttackShape.Projectile) FireProjectile(ability, damage);
        else MeleeHit(ability, damage);
    }

    void MeleeHit(AbilityData ability, float damage)
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(OffsetPoint(ability), ability.hitboxSize, 0f, hitMask);

        var alreadyHit = new HashSet<IDamageable>(); // one object can have several colliders
        foreach (Collider2D col in hits)
        {
            if (col.transform.IsChildOf(transform)) continue; // don't hit ourselves
            IDamageable target = col.GetComponentInParent<IDamageable>();
            if (target == null || !alreadyHit.Add(target)) continue;
            CombatResolver.Apply(gameObject, ability, damage, target);
        }
    }

    void FireProjectile(AbilityData ability, float damage)
    {
        if (ability.projectilePrefab == null)
        {
            Debug.LogWarning($"{ability.name} is a Projectile ability but has no projectile prefab.");
            return;
        }

        Projectile p = Instantiate(ability.projectilePrefab, OffsetPoint(ability), Quaternion.identity);
        p.Launch(gameObject, ability, damage, hitMask, new Vector2(Facing, 0f));
    }

    Vector2 OffsetPoint(AbilityData ability)
    {
        return (Vector2)transform.position +
               new Vector2(ability.hitboxOffset.x * Facing, ability.hitboxOffset.y);
    }

    void OnDrawGizmosSelected()
    {
        AbilityData a = basicAttack;
        if (a == null) return;
        Gizmos.color = Color.red;
        if (a.shape == AttackShape.Melee) Gizmos.DrawWireCube(OffsetPoint(a), a.hitboxSize);
        else Gizmos.DrawWireSphere(OffsetPoint(a), 0.15f);
    }
}
