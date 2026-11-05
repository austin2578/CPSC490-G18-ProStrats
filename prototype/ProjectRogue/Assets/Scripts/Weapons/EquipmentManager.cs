using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour, IWeaponSource
{
    public const int SkillSlotCount = 3;

    public WeaponData startingWeapon;

    public WeaponInstance Current { get; private set; }
    public AbilityData[] SkillSlots { get; private set; } = new AbilityData[SkillSlotCount];

    // One saved skill loadout per family, so swapping away and back loses nothing.
    readonly Dictionary<WeaponFamily, AbilityData[]> loadouts = new Dictionary<WeaponFamily, AbilityData[]>();

    public event Action<WeaponInstance, WeaponInstance> WeaponChanged; // (previous, next)

    void Start()
    {
        if (startingWeapon != null) Equip(new WeaponInstance(startingWeapon));
    }

    public void Equip(WeaponInstance next)
    {
        WeaponInstance previous = Current;
        if (previous != null) loadouts[previous.Data.family] = SkillSlots;

        Current = next;
        SkillSlots = loadouts.TryGetValue(next.Data.family, out AbilityData[] saved)
            ? saved
            : new AbilityData[SkillSlotCount];

        WeaponChanged?.Invoke(previous, next);
    }

    public bool TrySetSkill(int slot, AbilityData skill)
    {
        if (Current == null || slot < 0 || slot >= SkillSlotCount) return false;

        // Passing null clears the slot. Otherwise the skill must belong to the current weapon's family.
        if (skill != null && (Current.Data.skills == null || !Array.Exists(Current.Data.skills, s => s == skill)))
            return false;

        SkillSlots[slot] = skill;
        return true;
    }

    // IWeaponSource (lets PlayerCombat from the Combat folder use this without depending on it)
    public bool HasWeapon => Current != null;
    public AbilityData BasicAttack => Current != null ? Current.Data.basicAttack : null;
    public AbilityData GetSkill(int slot) => (slot >= 0 && slot < SkillSlotCount) ? SkillSlots[slot] : null;
    public float GetBaseDamage(AttributeSet attrs) => Current != null ? Current.GetDamage(attrs) : 0f;
}
