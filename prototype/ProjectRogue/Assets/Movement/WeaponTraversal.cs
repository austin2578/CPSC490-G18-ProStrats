using UnityEngine;

namespace ProjectRogue.Movement
{
    public enum WeaponKind { Sword, Axe, Bow }

    [RequireComponent(typeof(PlayerMovement))]
    public sealed class WeaponTraversal : MonoBehaviour
    {
        [SerializeField] private WeaponKind selectedWeapon;
        [Header("Sword - Horizontal Dash")]
        [SerializeField, Min(0.05f), Tooltip("Unobstructed dash distance in world units. Walls can stop it early.")]
        private float dashDistance = 3f;
        [SerializeField, Min(0.1f), Tooltip("World units per second. Duration is distance divided by speed.")]
        private float dashSpeed = 15f;
        [SerializeField, Min(0f)] private float dashCooldown = 0.75f;
        [Header("Axe - Airborne Downward Slam")]
        [SerializeField, Min(0.1f), Tooltip("Downward world units per second, independent of the normal fall-speed limit.")]
        private float slamSpeed = 20f;
        [SerializeField, Min(0f)] private float slamCooldown = 0.75f;
        [Header("Bow - Grounded Backward Hop")]
        [SerializeField, Min(0.05f), Tooltip("Unobstructed horizontal distance opposite the facing direction.")]
        private float hopDistance = 2.5f;
        [SerializeField, Min(0.1f)] private float hopSpeed = 8f;
        [SerializeField, Min(0.05f), Tooltip("Approximate rise above takeoff in world units.")]
        private float hopHeight = 1f;
        [SerializeField, Min(0f)] private float hopCooldown = 0.8f;

        private enum Motion { None, Dash, Slam, Hop }
        private Motion active;
        private bool requested;
        private bool usedAirDash;
        private float remainingDistance;
        private float cooldown;
        private int direction;
        public WeaponKind SelectedWeapon => selectedWeapon;
        public bool IsUsingUtility => active != Motion.None;
        public float CooldownRemaining => cooldown;

        public void RequestUtility() => requested = true;
        public void SelectWeapon(int index)
        {
            if (index < 0 || index > 2) return;
            selectedWeapon = (WeaponKind)index;
            active = Motion.None;
            requested = false;
            // Switching does not erase cooldowns or grant another midair dash.
        }

        // PlayerMovement owns the final Rigidbody velocity; abilities only propose motion.
        public bool Step(bool grounded, int facing, float normalGravity, float dt,
            ref Vector2 velocity, out float gravity)
        {
            gravity = normalGravity;
            cooldown = Mathf.Max(0, cooldown - dt);
            if (grounded) usedAirDash = false;
            if (active == Motion.Slam && grounded) active = Motion.None;
            if ((active == Motion.Dash || active == Motion.Hop) && remainingDistance <= 0)
            {
                active = Motion.None;
                velocity.x = 0;
            }
            if (requested && active == Motion.None && cooldown <= 0)
            {
                direction = facing;
                if (selectedWeapon == WeaponKind.Sword && (grounded || !usedAirDash))
                {
                    active = Motion.Dash;
                    remainingDistance = dashDistance;
                    cooldown = dashCooldown;
                    usedAirDash = true;
                }
                else if (selectedWeapon == WeaponKind.Axe && !grounded)
                {
                    active = Motion.Slam;
                    cooldown = slamCooldown;
                }
                else if (selectedWeapon == WeaponKind.Bow && grounded)
                {
                    active = Motion.Hop;
                    direction = -facing;
                    remainingDistance = hopDistance;
                    cooldown = hopCooldown;
                    velocity.y = Mathf.Sqrt(2f * Mathf.Abs(Physics2D.gravity.y) * normalGravity * hopHeight);
                }
            }
            requested = false;
            if (active == Motion.None) return false;
            if (active == Motion.Slam)
            {
                gravity = 0;
                velocity = new Vector2(0, -slamSpeed);
                return true;
            }
            float speed = active == Motion.Dash ? dashSpeed : hopSpeed;
            float travel = Mathf.Min(speed * dt, remainingDistance);
            velocity.x = direction * travel / dt;
            remainingDistance = Mathf.Max(0, remainingDistance - travel);
            if (active == Motion.Dash)
            {
                gravity = 0;
                velocity.y = 0;
            }
            return true;
        }

        public void ResetState()
        {
            active = Motion.None;
            requested = false;
            remainingDistance = 0;
            cooldown = 0;
            usedAirDash = false;
        }
        private void OnDisable() => ResetState();
    }
}
