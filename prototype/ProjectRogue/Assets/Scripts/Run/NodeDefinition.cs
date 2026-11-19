using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectRogue.Run
{
    // One option inside a node; shop item, mystery event answer, rest service, etc
    [Serializable]
    public class NodeChoice
    {
        public string label = "Choice";
        [TextArea] public string description;
        [Min(0)] public int goldCost;
        public List<RunRequirement> requirements = new List<RunRequirement>();
        public List<RunReward> rewards = new List<RunReward>();
        [Tooltip("Can be taken more than once in the same visit (e.g. a stack of potions).")]
        public bool repeatable;
    }

    // Data for one type of node; map places instances of these when generated w/ each node linking back to one type.
    [CreateAssetMenu(menuName = "Rogue/Run Node")]
    public class NodeDefinition : ScriptableObject
    {
        [Tooltip("Stable id used for save data and instance ids. Defaults to the asset name.")]
        public string id;
        public string displayName = "Node";
        public NodeType nodeType = NodeType.Combat;
        [TextArea] public string description;

        [Header("Route gate (checked before entering)")]
        public List<RunRequirement> entryRequirements = new List<RunRequirement>();

        [Header("Granted once when the node is completed")]
        public List<RunReward> completionRewards = new List<RunReward>();

        [Header("Optional choices taken while inside the node")]
        public List<NodeChoice> choices = new List<NodeChoice>();
        [Min(0), Tooltip("How many different choices may be taken per visit. 0 = unlimited (shops).")]
        public int maxChoices = 1;

        public string Id => string.IsNullOrEmpty(id) ? name : id;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id)) id = name;
        }
    }
}
