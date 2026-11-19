using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectRogue.Run
{
    // A physical door for one node of current branch
    // at open door and press E to commit to that node
    // color shows state: green = open, red = locked by a requirement, gray = no node
    [RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public sealed class RunNodeDoor : MonoBehaviour
    {
        [SerializeField] private RunDebugPanel route;
        [SerializeField, Min(0)] private int branchIndex;
        [SerializeField] private TextMesh label;

        private static readonly Color Open = new Color(0.25f, 0.8f, 0.35f);
        private static readonly Color Locked = new Color(0.85f, 0.25f, 0.25f);
        private static readonly Color Idle = new Color(0.35f, 0.35f, 0.4f);

        private SpriteRenderer sprite;
        private int playerOverlaps;

        public NodeDefinition Node =>
            route != null && branchIndex < route.Branch.Count ? route.Branch[branchIndex] : null;

        private void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
            GetComponent<BoxCollider2D>().isTrigger = true;
            if (route == null) route = FindFirstObjectByType<RunDebugPanel>();
        }

        private void Update()
        {
            NodeDefinition node = Node;
            RunSession s = route != null ? route.Session : null;
            string text;
            Color color;
            if (node == null || s == null)
            {
                text = route != null && route.ActCleared ? "Act cleared" : "";
                color = Idle;
            }
            else if (s.InNode)
            {
                text = s.CurrentVisit.Definition == node ? $"{node.displayName}\n(inside)" : node.displayName;
                color = Idle;
            }
            else if (s.CanEnter(node, route.InstanceIdFor(node), out string reason))
            {
                text = $"{node.displayName}\n{RewardText.Join(node.completionRewards)}";
                color = Open;
            }
            else
            {
                text = $"{node.displayName}\n{reason}";
                color = Locked;
            }
            sprite.color = color;
            if (label != null) label.text = text;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsPlayerCollider(other)) playerOverlaps++;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (IsPlayerCollider(other)) playerOverlaps = Mathf.Max(0, playerOverlaps - 1);
        }

        private void LateUpdate()
        {
            if (playerOverlaps == 0 || !InteractPressed()) return;
            NodeDefinition node = Node;
            if (node == null || route == null || route.Session == null || route.Session.InNode) return;
            route.TryEnter(node);
        }

        private static bool IsPlayerCollider(Collider2D other)
        {
            RunManager run = RunManager.Instance;
            if (run == null) return false;
            GameObject who = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
            return run.IsPlayer(who);
        }

        private static bool InteractPressed()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard kb = Keyboard.current;
            return kb != null && kb.eKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.E);
#endif
        }
    }
}
