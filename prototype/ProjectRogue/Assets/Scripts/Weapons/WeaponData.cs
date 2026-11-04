using UnityEngine;

[System.Serializable]
public struct AttributeScaling
{
    public AttributeType attribute;
    public ScalingGrade grade;
}

// Static definition of a weapon type (the asset). Per-drop randomness lives in WeaponInstance.
[CreateAssetMenu(menuName = "Rogue/Weapon")]
public class WeaponData : ScriptableObject
{
    public string displayName;
    public WeaponFamily family;
    public float baseDamage = 10f;

    [Header("S/A/B/C/D/E scaling, one entry per attribute that matters")]
    public AttributeScaling[] scaling;

    [Header("Combat")]
    public AbilityData basicAttack;
    public AbilityData[] skills; // skills this weapon's family can equip

    [Header("Affix rolling")]
    public AffixData[] affixPool;
    public int minAffixes = 1, maxAffixes = 3;
}
