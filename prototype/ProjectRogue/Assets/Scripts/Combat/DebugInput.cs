using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public enum DebugKey { Basic, Ability1, Skill1, Skill2, Skill3, SwapWeapon, CycleLoadout }

// Temporary keyboard input for testing.
public static class DebugInput
{
    public static bool Pressed(DebugKey key)
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb == null) return false;
        switch (key)
        {
            case DebugKey.Basic: return kb.jKey.wasPressedThisFrame;
            case DebugKey.Ability1: return kb.kKey.wasPressedThisFrame;
            case DebugKey.Skill1: return kb.digit1Key.wasPressedThisFrame;
            case DebugKey.Skill2: return kb.digit2Key.wasPressedThisFrame;
            case DebugKey.Skill3: return kb.digit3Key.wasPressedThisFrame;
            case DebugKey.SwapWeapon: return kb.nKey.wasPressedThisFrame;
            case DebugKey.CycleLoadout: return kb.mKey.wasPressedThisFrame;
        }
        return false;
#else
        switch (key)
        {
            case DebugKey.Basic: return Input.GetKeyDown(KeyCode.J);
            case DebugKey.Ability1: return Input.GetKeyDown(KeyCode.K);
            case DebugKey.Skill1: return Input.GetKeyDown(KeyCode.Alpha1);
            case DebugKey.Skill2: return Input.GetKeyDown(KeyCode.Alpha2);
            case DebugKey.Skill3: return Input.GetKeyDown(KeyCode.Alpha3);
            case DebugKey.SwapWeapon: return Input.GetKeyDown(KeyCode.N);
            case DebugKey.CycleLoadout: return Input.GetKeyDown(KeyCode.M);
        }
        return false;
#endif
    }
}
