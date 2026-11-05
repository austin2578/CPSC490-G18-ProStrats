using System.Collections.Generic;
using UnityEngine;

// A specific dropped weapon: a WeaponData plus its rolled affixes.
// Plain C# class (not a MonoBehaviour), so create it with new WeaponInstance(data).
public class WeaponInstance
{
    public WeaponData Data { get; }
    public IReadOnlyList<RolledAffix> Affixes => affixes;
    readonly List<RolledAffix> affixes = new List<RolledAffix>();

    public WeaponInstance(WeaponData data)
    {
        Data = data;
        RollAffixes();
    }

    void RollAffixes()
    {
        if (Data.affixPool == null || Data.affixPool.Length == 0) return;

        int count = Mathf.Min(Random.Range(Data.minAffixes, Data.maxAffixes + 1), Data.affixPool.Length);
        var pool = new List<AffixData>(Data.affixPool);

        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, pool.Count);
            AffixData picked = pool[idx];
            pool.RemoveAt(idx); // no duplicate affixes on one weapon
            affixes.Add(new RolledAffix { data = picked, value = Random.Range(picked.min, picked.max) });
        }
    }

    // Base damage plus S/A/B/C/D/E attribute scaling (no affixes).
    public float GetScaledBase(AttributeSet attrs)
    {
        float dmg = Data.baseDamage;

        if (attrs != null && Data.scaling != null)
            foreach (AttributeScaling s in Data.scaling)
                dmg += attrs.Get(s.attribute) * s.grade.Multiplier();

        return dmg;
    }

    // (base + scaling + flat affixes) * (1 + percent affixes). PlayerCombat then runs this
    // through CombatResolver.ComputeDamage like any other base damage.
    public float GetDamage(AttributeSet attrs)
    {
        float dmg = GetScaledBase(attrs);

        float percent = 0f;
        foreach (RolledAffix a in affixes)
        {
            if (a.data.stat == AffixStat.FlatDamage) dmg += a.value;
            else if (a.data.stat == AffixStat.PercentDamage) percent += a.value;
        }

        return dmg * (1f + percent / 100f);
    }
}