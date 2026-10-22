using UnityEngine;

public enum AttackShape { Melee, Projectile }

// Basic attacks are just abilities with cost 0.
// Sword abilities are Melee; bow and wand abilities are Projectile.
[CreateAssetMenu(menuName = "Rogue/Ability")]
public class AbilityData : ScriptableObject
{
    public string displayName;
    public AttackShape shape = AttackShape.Melee;
    public ResourceType resource = ResourceType.None;
    public float cost = 0f;
    public float cooldown = 0.4f;
    public float damageMultiplier = 1f;
    public DamageType damageType = DamageType.Physical;
    public string[] tags;

    [Header("Melee hitbox / projectile spawn point (relative to attacker, mirrored by facing)")]
    public Vector2 hitboxSize = new Vector2(1.5f, 1f);   // melee only
    public Vector2 hitboxOffset = new Vector2(1f, 0f);

    [Header("Projectile only")]
    public Projectile projectilePrefab;
    public float projectileSpeed = 12f;
    public float projectileLifetime = 2f;
}
