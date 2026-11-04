using System;
using UnityEngine;

// event boons respond to.
public static class CombatEvents
{
    public delegate void PreDamageHandler(ref DamageInfo info);

    public static event PreDamageHandler PreDamage;                      // last chance to modify a hit
    public static event Action<DamageInfo> Hit, Crit, Kill;
    public static event Action<GameObject, EffectContext> Dodged;         // who dodged
    public static event Action<GameObject, StatusInstance, EffectContext> StatusApplied; // target, status

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() { PreDamage = null; Hit = null; Crit = null; Kill = null; Dodged = null; StatusApplied = null; }

    public static void RaisePreDamage(ref DamageInfo i) => PreDamage?.Invoke(ref i);
    public static void RaiseHit(DamageInfo i) => Hit?.Invoke(i);
    public static void RaiseCrit(DamageInfo i) => Crit?.Invoke(i);
    public static void RaiseKill(DamageInfo i) => Kill?.Invoke(i);
    public static void RaiseDodged(GameObject who, EffectContext c) => Dodged?.Invoke(who, c);
    public static void RaiseStatusApplied(GameObject target, StatusInstance s, EffectContext c) => StatusApplied?.Invoke(target, s, c);
}