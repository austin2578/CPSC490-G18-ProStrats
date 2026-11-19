using ProjectRogue.Movement;
using UnityEngine;

[CreateAssetMenu(
    menuName = "Rogue/Class Definition",
    fileName = "NewClassDefinition")]
public sealed class ClassDefinition : ScriptableObject
{
    [Header("Identity")]
    public string displayName = "Class";
    public ClassKind classKind;

    [Header("Starting Attributes")]
    [Min(0)] public int vigor = 5;
    [Min(0)] public int strength = 5;
    [Min(0)] public int dexterity = 5;
    [Min(0)] public int mind = 5;
    [Min(0)] public int constitution = 5;

    [Header("Starting Resources")]
    [Min(1f)] public float maxHealth = 100f;
    [Min(0f)] public float maxStamina = 100f;
    [Min(0f)] public float maxMana = 50f;
}