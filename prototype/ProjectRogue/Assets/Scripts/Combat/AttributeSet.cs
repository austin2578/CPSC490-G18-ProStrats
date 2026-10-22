using UnityEngine;

public enum AttributeType { Vigor, Strength, Dexterity, Mind, Constitution }

// TEMPORARY STUB for Objective 1.2 (owned by a teammate).
// Delete or merge this when their attribute code lands; keep the Get() signature
// so weapon scaling keeps working.
public class AttributeSet : MonoBehaviour
{
    public int vigor = 5, strength = 5, dexterity = 5, mind = 5, constitution = 5;

    public int Get(AttributeType type)
    {
        switch (type)
        {
            case AttributeType.Vigor: return vigor;
            case AttributeType.Strength: return strength;
            case AttributeType.Dexterity: return dexterity;
            case AttributeType.Mind: return mind;
            case AttributeType.Constitution: return constitution;
            default: return 0;
        }
    }
}
