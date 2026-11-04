using System.Collections.Generic;
using UnityEngine;

// apply on the player (and any enemy that should have crit / resistances / modifiers) if needed.
[DefaultExecutionOrder(-100)] // seed base values before other components' Awake func reads them
public class CombatStats : MonoBehaviour
{
    [Header("Base values (boons/statuses modify these)")]
    [SerializeField] float critChance = 0.05f;
    [SerializeField] float critMultiplier = 1.5f;
    [SerializeField] float attackSpeed = 1f;
    [SerializeField] float resourceCostMultiplier = 1f;

    // set to true during dodge i-frames to trigger "On-Dodge" boons.
    public bool IsInvulnerable { get; set; }

    readonly Dictionary<StatId, Stat> stats = new Dictionary<StatId, Stat>();
    readonly Dictionary<StatId, float> bases = new Dictionary<StatId, float>();

    void Awake()
    {
        bases[StatId.CritChance] = critChance;
        bases[StatId.CritMultiplier] = critMultiplier;
        bases[StatId.AttackSpeed] = attackSpeed;
        bases[StatId.ResourceCostMultiplier] = resourceCostMultiplier;
    }

    Stat GetStat(StatId id)
    {
        if (!stats.TryGetValue(id, out var s)) { s = new Stat(); stats[id] = s; }
        return s;
    }

    // final value of a stat owned by CombatStats (crit, attack spd, bonuses, etc)
    public float Get(StatId id)
    {
        bases.TryGetValue(id, out float b);
        return GetStat(id).Evaluate(b);
    }

    public void AddModifier(StatModifier m) => GetStat(m.Stat).Add(m);

    public void RemoveFromSource(object source)
    { foreach (var s in stats.Values) s.RemoveFromSource(source); }
}