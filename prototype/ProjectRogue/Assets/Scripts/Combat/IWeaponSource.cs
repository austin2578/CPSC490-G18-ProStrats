// Lets PlayerCombat use a weapon system without depending on it.
// The Combat folder compiles on its own (Objective 2.2); the Weapons folder (Objective 1.3)
public interface IWeaponSource
{
    bool HasWeapon { get; }
    AbilityData BasicAttack { get; }
    AbilityData GetSkill(int slot);
    float GetBaseDamage(AttributeSet attributes);
}
