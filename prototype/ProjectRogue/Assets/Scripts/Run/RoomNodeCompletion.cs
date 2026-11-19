using ProjectRogue.Movement;
using UnityEngine;

namespace ProjectRogue.Run
{
    // Connect existing TraversalRoom to the run; reaching the exit marks current node as complete
    public sealed class RoomNodeCompletion : MonoBehaviour
    {
        [SerializeField] private TraversalRoom room;
        [SerializeField] private NodeDefinition node;
        [SerializeField, Tooltip("Leave empty to use the node id.")] private string instanceId;

        private bool granted;

        private void Update()
        {
            if (granted || room == null || node == null || !room.IsComplete) return;
            RunManager run = RunManager.Instance;
            if (run == null || run.Session == null) return;
            string id = string.IsNullOrEmpty(instanceId) ? node.Id : instanceId;
            run.Session.CompleteNode(node, id);
            granted = true; // restarting the room after completion grants nothing more
        }
    }
}
