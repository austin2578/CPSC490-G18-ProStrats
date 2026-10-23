using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectRogue.Movement
{
    [RequireComponent(typeof(PlayerMovement))]
    public sealed class PlayerMovementInput : MonoBehaviour
    {
        [SerializeField] private InputActionAsset actions;
        private InputActionAsset instance;
        private InputAction move;
        private InputAction jump;
        private InputAction utility;
        private WeaponTraversal traversal;
        private PlayerMovement movement;

        private void OnEnable()
        {
            movement = GetComponent<PlayerMovement>();
            traversal = GetComponent<WeaponTraversal>();
            if (actions == null)
            {
                Debug.LogError("Assign the team's input action asset before enabling movement input.", this);
                enabled = false;
                return;
            }
            // Own a copy so disabling this player cannot disable another player's bindings.
            instance = Instantiate(actions);
            move = instance.FindAction("Player/Move", true);
            jump = instance.FindAction("Player/Jump", true);
            move.Enable();
            jump.Enable();
            utility = new InputAction("Traversal Utility", InputActionType.Button, "<Keyboard>/leftShift");
            utility.AddBinding("<Gamepad>/rightShoulder");
            utility.Enable();
        }

        private void Update()
        {
            movement.SetHorizontalInput(move.ReadValue<Vector2>().x);
            if (jump.WasPressedThisFrame()) movement.RequestJump();
            if (utility.WasPressedThisFrame() && traversal != null) traversal.RequestUtility();
        }

        private void OnDisable()
        {
            utility?.Dispose();
            utility = null;
            if (movement != null) movement.SetHorizontalInput(0f);
            if (instance != null)
            {
                instance.Disable();
                Destroy(instance);
            }
        }
    }
}
