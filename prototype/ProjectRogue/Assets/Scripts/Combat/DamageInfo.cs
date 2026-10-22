using UnityEngine;

public enum DamageType { Physical, Fire, Ice, Lightning, Arcane}


// Everytgubg about one hit. Boons/ Status will read the tags
public struct DamageInfo
{
    public GameObject source;
    public float amount;
    public DamageType type;
    public string[] tags;
}