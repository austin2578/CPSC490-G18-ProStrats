using System.Linq;
using UnityEngine;

//   N = equip a freshly rolled copy of the next weapon in the list.
// Keys 1/2/3 use the equipped skill slots (handled by PlayerCombat).
[RequireComponent(typeof(EquipmentManager))]
public class TestWeaponSwapper : MonoBehaviour
{
    public WeaponData[] weapons;

    int index;
    EquipmentManager equipment;
    AttributeSet attrs;
    MasteryTracker mastery;

    void Awake()
    {
        equipment = GetComponent<EquipmentManager>();
        attrs = GetComponent<AttributeSet>();
        mastery = GetComponent<MasteryTracker>();
        equipment.WeaponChanged += OnWeaponChanged; // subscribe before EquipmentManager.Start equips
    }

    void OnDestroy()
    {
        if (equipment != null) equipment.WeaponChanged -= OnWeaponChanged;
    }

    void Update()
    {
        if (!DebugInput.Pressed(DebugKey.SwapWeapon) || weapons == null || weapons.Length == 0) return;
        equipment.Equip(new WeaponInstance(weapons[index++ % weapons.Length]));
    }

    // Fill any empty skill slots with the new weapon's first skills so keys 1/2/3 work right away.
    // Slots restored from a saved loadout are not empty, so they are left alone.
    void OnWeaponChanged(WeaponInstance previous, WeaponInstance next)
    {
        AbilityData[] skills = next.Data.skills;
        if (skills != null)
            for (int i = 0; i < EquipmentManager.SkillSlotCount && i < skills.Length; i++)
                if (equipment.SkillSlots[i] == null) equipment.TrySetSkill(i, skills[i]);

        LogState("Equipped");
    }

    void LogState(string label)
    {
        WeaponInstance w = equipment.Current;
        if (w == null) { Debug.Log($"{label}: no weapon"); return; }

        string affixes = w.Affixes.Count == 0
            ? "none"
            : string.Join(", ", w.Affixes.Select(a => $"{a.data.name} {a.value:0.#}"));
        string slots = string.Join(", ", equipment.SkillSlots.Select(s => s == null ? "(empty)" : s.name));
        string masteryText = mastery != null
            ? $"Mastery level {mastery.GetLevel(w.Data.family)} ({mastery.GetXp(w.Data.family)} xp)"
            : "no MasteryTracker";

        Debug.Log($"{label}: {w.Data.name} [{w.Data.family}] | scaled base {w.GetScaledBase(attrs):0.#} " +
                  $"| with affixes {w.GetDamage(attrs):0.#} | affixes: {affixes} | slots: {slots} | {masteryText}");
    }

    [ContextMenu("Test: log current state")]
    void LogCurrentState() => LogState("State");

    [ContextMenu("Test: roll 5 copies of the first weapon")]
    void RollFiveCopies()
    {
        if (weapons == null || weapons.Length == 0) return;
        for (int i = 0; i < 5; i++)
        {
            var w = new WeaponInstance(weapons[0]);
            string affixes = w.Affixes.Count == 0
                ? "none"
                : string.Join(", ", w.Affixes.Select(a => $"{a.data.name} {a.value:0.#}"));
            Debug.Log($"Copy {i + 1} of {weapons[0].name}: {affixes} | damage {w.GetDamage(attrs):0.#}");
        }
    }

    [ContextMenu("Test: set a 4th skill slot (should FAIL)")]
    void TestFourthSlot()
    {
        if (equipment.Current == null || equipment.Current.Data.skills == null || equipment.Current.Data.skills.Length == 0) return;
        bool ok = equipment.TrySetSkill(EquipmentManager.SkillSlotCount, equipment.Current.Data.skills[0]);
        Debug.Log($"TrySetSkill(slot {EquipmentManager.SkillSlotCount}) -> {ok} (expected False)");
    }

    [ContextMenu("Test: equip a skill from another family (should FAIL)")]
    void TestWrongFamily()
    {
        if (equipment.Current == null) return;
        WeaponData other = weapons.FirstOrDefault(x => x != null && x.family != equipment.Current.Data.family
                                                       && x.skills != null && x.skills.Length > 0);
        if (other == null) { Debug.Log("Add a weapon from a different family to the list first."); return; }
        bool ok = equipment.TrySetSkill(0, other.skills[0]);
        Debug.Log($"TrySetSkill(0, {other.skills[0].name} from {other.family}) while holding " +
                  $"{equipment.Current.Data.family} -> {ok} (expected False)");
    }
}