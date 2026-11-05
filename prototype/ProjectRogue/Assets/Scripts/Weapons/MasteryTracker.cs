using System;
using System.Collections.Generic;
using UnityEngine;

// Mastery is tracked per weapon FAMILY on the player, not on the weapon.
// That is what lets it survive swapping to another weapon of the same family.
// Mastery XP is earned by landing hits with the equipped weapon's family.
public class MasteryTracker : MonoBehaviour
{
    public const int XpPerLevel = 100; // placeholder
    public int xpPerHit = 5;           // placeholder

    readonly Dictionary<WeaponFamily, int> xp = new Dictionary<WeaponFamily, int>();
    EquipmentManager equipment;

    public event Action<WeaponFamily, int> LevelUp;

    void Awake() { equipment = GetComponent<EquipmentManager>(); }
    void OnEnable() { CombatResolver.DamageResolved += OnDamageResolved; }
    void OnDisable() { CombatResolver.DamageResolved -= OnDamageResolved; }

    void OnDamageResolved(DamageInfo info, IDamageable target)
    {
        if (equipment == null || equipment.Current == null) return;
        if (info.source != gameObject) return; // only hits we dealt
        AddXp(equipment.Current.Data.family, xpPerHit);
    }

    public int GetXp(WeaponFamily family) => xp.TryGetValue(family, out int v) ? v : 0;
    public int GetLevel(WeaponFamily family) => GetXp(family) / XpPerLevel;

    public void AddXp(WeaponFamily family, int amount)
    {
        int before = GetLevel(family);
        xp[family] = GetXp(family) + amount;
        int after = GetLevel(family);
        if (after > before)
        {
            Debug.Log($"{family} Mastery reached level {after}");
            LevelUp?.Invoke(family, after);
        }
    }

    public void ResetRun() => xp.Clear(); // call when a new run starts

    [ContextMenu("Log mastery")]
    void LogMastery()
    {
        foreach (WeaponFamily f in Enum.GetValues(typeof(WeaponFamily)))
            Debug.Log($"{f}: level {GetLevel(f)} ({GetXp(f)} xp)");
    }
}