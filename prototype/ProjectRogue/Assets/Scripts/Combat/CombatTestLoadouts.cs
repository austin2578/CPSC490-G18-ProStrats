using UnityEngine;

// Fill in one loadout per weapon (Sword, Bow, Wand), then press M in play mode to cycle.
// It just sets PlayerCombat's basic attack and ability1. Delete this once 1.3's weapons
// supply the abilities themselves.
[RequireComponent(typeof(PlayerCombat))]
public class CombatTestLoadouts : MonoBehaviour
{
    [System.Serializable]
    public struct Loadout
    {
        public string name;
        public AbilityData basic;
        public AbilityData skill;
    }

    public Loadout[] loadouts;

    int index = -1;
    PlayerCombat combat;

    void Awake() { combat = GetComponent<PlayerCombat>(); }
    void Start() { Cycle(); }

    void Update()
    {
        if (DebugInput.Pressed(DebugKey.CycleLoadout)) Cycle();
    }

    void Cycle()
    {
        if (loadouts == null || loadouts.Length == 0) return;
        index = (index + 1) % loadouts.Length;
        combat.basicAttack = loadouts[index].basic;
        combat.ability1 = loadouts[index].skill;
        Debug.Log($"Test loadout: {loadouts[index].name}");
    }
}
