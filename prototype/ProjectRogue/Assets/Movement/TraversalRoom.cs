using UnityEngine;

namespace ProjectRogue.Movement
{
    public sealed class TraversalRoom : MonoBehaviour
    {
        [SerializeField] private PlayerMovement player;
        [SerializeField] private Transform entrance;
        [SerializeField] private BoxCollider2D exitVolume;
        [SerializeField] private SpriteRenderer exitMarker;
        [SerializeField] private float fallThreshold = -5f;
        private Rigidbody2D body;
        private CapsuleCollider2D capsule;

        public bool IsComplete { get; private set; }
        public int FallCount { get; private set; }

        private void Awake()
        {
            if (player == null || entrance == null || exitVolume == null || exitMarker == null)
            {
                Debug.LogError("Traversal room references must be assigned.", this);
                enabled = false;
                return;
            }
            body = player.GetComponent<Rigidbody2D>();
            capsule = player.GetComponent<CapsuleCollider2D>();
            exitMarker.color = Color.yellow;
        }

        private void FixedUpdate()
        {
            if (body.position.y < fallThreshold)
            {
                FallCount++;
                RestartRoom();
                return;
            }
            if (!IsComplete && exitVolume.Distance(capsule).isOverlapped)
            {
                IsComplete = true;
                exitMarker.color = Color.green;
                Debug.Log("Traversal room complete: entrance to exit reached.", this);
            }
        }

        [ContextMenu("Restart Room")]
        public void RestartRoom()
        {
            if (body == null) return;
            player.ResetMotion();
            body.position = entrance.position;
            body.rotation = 0f;
            // Also reset the interpolation pose so respawning does not sweep across the room.
            player.transform.position = entrance.position;
            IsComplete = false;
            exitMarker.color = Color.yellow;
        }
    }
}
