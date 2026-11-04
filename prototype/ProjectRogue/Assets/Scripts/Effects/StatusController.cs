using System.Collections.Generic;
using UnityEngine;

// apply on anything that can be afflicted (player or ebemy)
public class StatusController : MonoBehaviour
{
    CombatStats stats;
    IDamageable damageable;
    readonly Dictionary<StatusDefinition, StatusInstance> active = new Dictionary<StatusDefinition, StatusInstance>();
    readonly List<StatusInstance> scratch = new List<StatusInstance>();

    void Awake()
    {
        stats = GetComponent<CombatStats>();
        damageable = GetComponent<IDamageable>();
    }

    public int StacksOf(StatusDefinition def) => active.TryGetValue(def, out var i) ? i.Stacks : 0;
    public bool Has(StatusDefinition def) => active.ContainsKey(def);

    public void Apply(StatusDefinition def, int stacks, EffectContext ctx)
    {
        if (def == null || (damageable != null && CombatResolver.IsDead(damageable))) return;
        if (!active.TryGetValue(def, out var inst)) { inst = new StatusInstance(def); active.Add(def, inst); }

        inst.Stacks = Mathf.Min(def.maxStacks, inst.Stacks + stacks);
        inst.Remaining = def.duration;
        inst.SourceContext = ctx;
        RefreshModifiers(inst);
        CombatEvents.RaiseStatusApplied(gameObject, inst, ctx);

        if (def.thresholdStacks > 0 && inst.Stacks >= def.thresholdStacks && def.thresholdEffect != null)
        {
            var child = ctx.CreateChild(def.Id + " (threshold)", OriginType.StatusThreshold);
            if (def.clearOnThreshold) Remove(inst);
            def.thresholdEffect.Execute(child, ctx.Source, gameObject, 0f);
        }
    }

    void RefreshModifiers(StatusInstance inst)
    {
        if (stats == null) return;
        stats.RemoveFromSource(inst);
        foreach (var m in inst.Def.holderModifiers) stats.AddModifier(m.Create(inst, inst.Stacks));
    }

    void Remove(StatusInstance inst)
    {
        if (stats != null) stats.RemoveFromSource(inst);
        active.Remove(inst.Def);
    }

    void Update()
    {
        if (active.Count == 0) return;
        float dt = Time.deltaTime;
        scratch.Clear(); scratch.AddRange(active.Values); // ticks can add/remove statuses

        foreach (var inst in scratch)
        {
            if (!active.ContainsKey(inst.Def)) continue;
            inst.Remaining -= dt;

            var d = inst.Def;
            if (d.tickDamageByStack != null && d.tickDamageByStack.Length > 0 && d.tickInterval > 0f)
            {
                inst.TickTimer += dt;
                while (inst.TickTimer >= d.tickInterval && (damageable == null || !CombatResolver.IsDead(damageable)))
                {
                    inst.TickTimer -= d.tickInterval;
                    Tick(inst);
                }
            }
            if (inst.Remaining <= 0f || (damageable != null && CombatResolver.IsDead(damageable))) Remove(inst);
        }
    }

    void Tick(StatusInstance inst)
    {
        if (damageable == null) return;
        var d = inst.Def;
        float dmg = d.tickDamageByStack[Mathf.Clamp(inst.Stacks - 1, 0, d.tickDamageByStack.Length - 1)];
        // ticks  damage-only by default (EffectContext.DefaultPermissions); wildfire style boons can give more
        var tickCtx = inst.SourceContext.CreateChild(d.Id, OriginType.StatusTick);
        CombatResolver.Resolve(inst.SourceContext.Source, d.tickDamageType, dmg, null, damageable, tickCtx);
    }
}