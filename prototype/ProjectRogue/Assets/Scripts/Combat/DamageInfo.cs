using UnityEngine;

public enum DamageType { Physical, Fire, Ice, Lightning, Arcane}


// Everytgubg about one hit. Boons/ Status will read the tags
public struct DamageInfo
{
    public GameObject source;
    public float amount;
    public DamageType type;
    public string[] tags;
    public GameObject target;      // added for effects; who was hit
    public bool isCrit;            // added for effects; crit dmg
    public EffectContext context; // added for effects; source chain, proc depth, permissions
}