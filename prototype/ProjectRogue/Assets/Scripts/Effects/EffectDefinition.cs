using UnityEngine;

// base class for reusable boons
public abstract class EffectDefinition : ScriptableObject
{
    // triggerValue = ex) the amount of dmg that caused specific event to happen
    public abstract void Execute(EffectContext ctx, GameObject owner, GameObject target, float triggerValue);
}