using UnityEngine;

namespace ProjectRogue.Movement
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 6f;
        [SerializeField, Min(0f)] private float acceleration = 50f;
        [SerializeField, HideInInspector] private float jumpSpeed = 11f;
        [SerializeField, HideInInspector] private bool jumpHeightMigrated;
        [SerializeField, Min(0.05f), Tooltip("Approximate height above takeoff in world units; gravity determines launch speed.")]
        private float jumpHeight = 2.055f;
        [SerializeField, Min(0.01f)] private float gravityScale = 3f;
        [SerializeField, Min(0f)] private float maxFallSpeed = 20f;
        [SerializeField, Min(0.001f)] private float groundDistance = 0.06f;
        [SerializeField] private LayerMask groundLayers = 1;

        private readonly RaycastHit2D[] groundHits = new RaycastHit2D[8];
        private Rigidbody2D body;
        private CapsuleCollider2D capsule;
        private float horizontalInput;
        private bool jumpRequested;
        private WeaponTraversal traversal;
        public int FacingSign { get; private set; } = 1;

        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            if (!jumpHeightMigrated)
            {
                float gravity = Mathf.Max(0.001f, Mathf.Abs(Physics2D.gravity.y) * gravityScale);
                jumpHeight = jumpSpeed * jumpSpeed / (2f * gravity);
                jumpHeightMigrated = true;
            }
            body = GetComponent<Rigidbody2D>();
            capsule = GetComponent<CapsuleCollider2D>();
            traversal = GetComponent<WeaponTraversal>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.freezeRotation = true;
            body.gravityScale = gravityScale;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        public void SetHorizontalInput(float value)
        {
            horizontalInput = Mathf.Clamp(value, -1f, 1f);
            if (Mathf.Abs(horizontalInput) > 0.01f) FacingSign = horizontalInput > 0 ? 1 : -1;
        }
        public void RequestJump() => jumpRequested = true;

        private void FixedUpdate()
        {
            Vector2 velocity = body.linearVelocity;
            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(groundLayers);
            int count = capsule.Cast(Vector2.down, filter, groundHits, groundDistance);
            IsGrounded = false;
            // A wall/ceiling cannot replenish jumps; upward travel cannot count as landing.
            if (velocity.y <= 0.01f)
                for (int i = 0; i < count; i++)
                    if (groundHits[i].normal.y >= 0.65f)
                    {
                        IsGrounded = true;
                        break;
                    }

            body.gravityScale = gravityScale;
            if (traversal != null && traversal.enabled &&
                traversal.Step(IsGrounded, FacingSign, gravityScale, Time.fixedDeltaTime,
                    ref velocity, out float abilityGravity))
            {
                body.gravityScale = abilityGravity;
                jumpRequested = false;
                body.linearVelocity = velocity;
                return;
            }

            velocity.x = Mathf.MoveTowards(velocity.x, horizontalInput * moveSpeed,
                acceleration * Time.fixedDeltaTime);
            velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
            if (jumpRequested && IsGrounded)
            {
                velocity.y = Mathf.Sqrt(2f * Mathf.Abs(Physics2D.gravity.y) * gravityScale * jumpHeight);
                IsGrounded = false;
            }
            jumpRequested = false;
            body.linearVelocity = velocity;
        }

        public void ResetMotion()
        {
            horizontalInput = 0f;
            jumpRequested = false;
            IsGrounded = false;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.gravityScale = gravityScale;
            if (traversal != null) traversal.ResetState();
        }

        private void OnDisable()
        {
            if (body != null) body.gravityScale = gravityScale;
            if (traversal != null) traversal.ResetState();
            horizontalInput = 0f;
            jumpRequested = false;
            IsGrounded = false;
        }
    }
}
