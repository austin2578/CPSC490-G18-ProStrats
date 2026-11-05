using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectRogue.Movement
{
    public enum ClassKind
    {
        Knight,
        Mage,
        Thief
    }

    [RequireComponent(typeof(PlayerMovement))]
    public sealed class ClassTraversal : MonoBehaviour
    {
        [FormerlySerializedAs("selectedWeapon")]
        [SerializeField] private ClassKind selectedClass;

        [Header("Knight - Horizontal Dash")]
        [SerializeField, Min(0.05f)]
        private float dashDistance = 3f;

        [SerializeField, Min(0.1f)]
        private float dashSpeed = 15f;

        [SerializeField, Min(0f)]
        private float dashCooldown = 0.75f;

        [Header("Mage - Airborne Downward Slam")]
        [SerializeField, Min(0.1f)]
        private float slamSpeed = 20f;

        [SerializeField, Min(0f)]
        private float slamCooldown = 0.75f;

        [Header("Thief - Grounded Backward Hop")]
        [SerializeField, Min(0.05f)]
        private float hopDistance = 2.5f;

        [SerializeField, Min(0.1f)]
        private float hopSpeed = 8f;

        [SerializeField, Min(0.05f)]
        private float hopHeight = 1f;

        [SerializeField, Min(0f)]
        private float hopCooldown = 0.8f;

        private enum Motion
        {
            None,
            Dash,
            Slam,
            Hop
        }

        private Motion active;
        private bool requested;
        private bool usedAirDash;
        private float remainingDistance;
        private float cooldown;
        private int direction;

        public ClassKind SelectedClass => selectedClass;
        public bool IsUsingUtility => active != Motion.None;
        public float CooldownRemaining => cooldown;

        public void RequestUtility()
        {
            requested = true;
        }

        public void SelectClass(int index)
        {
            if (index < 0 || index > 2)
                return;

            selectedClass = (ClassKind)index;
            active = Motion.None;
            requested = false;
        }

        public bool Step(
            bool grounded,
            int facing,
            float normalGravity,
            float dt,
            ref Vector2 velocity,
            out float gravity)
        {
            gravity = normalGravity;
            cooldown = Mathf.Max(0f, cooldown - dt);

            if (grounded)
                usedAirDash = false;

            if (active == Motion.Slam && grounded)
                active = Motion.None;

            if ((active == Motion.Dash || active == Motion.Hop) &&
                remainingDistance <= 0f)
            {
                active = Motion.None;
                velocity.x = 0f;
            }

            if (requested &&
                active == Motion.None &&
                cooldown <= 0f)
            {
                direction = facing;

                switch (selectedClass)
                {
                    case ClassKind.Knight:
                        if (grounded || !usedAirDash)
                        {
                            active = Motion.Dash;
                            remainingDistance = dashDistance;
                            cooldown = dashCooldown;
                            usedAirDash = true;
                        }
                        break;

                    case ClassKind.Mage:
                        if (!grounded)
                        {
                            active = Motion.Slam;
                            cooldown = slamCooldown;
                        }
                        break;

                    case ClassKind.Thief:
                        if (grounded)
                        {
                            active = Motion.Hop;
                            direction = -facing;
                            remainingDistance = hopDistance;
                            cooldown = hopCooldown;

                            velocity.y = Mathf.Sqrt(
                                2f *
                                Mathf.Abs(Physics2D.gravity.y) *
                                normalGravity *
                                hopHeight);
                        }
                        break;
                }
            }

            requested = false;

            if (active == Motion.None)
                return false;

            if (active == Motion.Slam)
            {
                gravity = 0f;
                velocity = new Vector2(0f, -slamSpeed);
                return true;
            }

            float speed = active == Motion.Dash
                ? dashSpeed
                : hopSpeed;

            float travel = Mathf.Min(speed * dt, remainingDistance);

            velocity.x = direction * travel / dt;
            remainingDistance =
                Mathf.Max(0f, remainingDistance - travel);

            if (active == Motion.Dash)
            {
                gravity = 0f;
                velocity.y = 0f;
            }

            return true;
        }

        public void ResetState()
        {
            active = Motion.None;
            requested = false;
            remainingDistance = 0f;
            cooldown = 0f;
            usedAirDash = false;
        }

        private void OnDisable()
        {
            ResetState();
        }
    }
}
