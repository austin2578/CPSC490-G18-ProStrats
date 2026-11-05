using UnityEngine;

// Both stat types are applied in WeaponInstance.GetDamage.
public enum AffixStat { FlatDamage, PercentDamage }

[CreateAssetMenu(menuName = "Rogue/Affix")]
public class AffixData : ScriptableObject
{
    public string displayName;
    public AffixStat stat;
    [Tooltip("Flat: damage points. Percent: percentage points (10 = +10%).")]
    public float min = 1f, max = 5f;
}

// One affix with its randomly rolled value, owned by a WeaponInstance.
[System.Serializable]
public class RolledAffix
{
    public AffixData data;
    public float value;
}
