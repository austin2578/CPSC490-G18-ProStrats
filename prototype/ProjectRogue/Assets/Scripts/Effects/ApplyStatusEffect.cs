using UnityEngine;

// example of one effect type
// applies a StatusDefinition (Burn, Bleed, Buff, etc) to target or player.
// also covers time-sensitive buffs
[CreateAssetMenu(menuName = "Rogue/Effects/Apply Status")]
public class ApplyStatusEffect : EffectDefinition
{
    public StatusDefinition status;
    public int stacks = 1;
    public bool applyToOwner;

    public override void Execute(EffectContext ctx, GameObject owner, GameObject target, float triggerValue)
    {
        var go = applyToOwner ? owner : target;
        if (go == null) return;
        if (!applyToOwner && !ctx.Permissions.HasFlag(ProcPermissions.CanApplyStatus)) return;
        var sc = go.GetComponent<StatusController>();
        if (sc != null) sc.Apply(status, stacks, ctx);
    }
}