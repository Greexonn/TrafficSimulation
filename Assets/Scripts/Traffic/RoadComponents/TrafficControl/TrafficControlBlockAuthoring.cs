using System.Collections.Generic;
using UnityEngine;

namespace TrafficSimulation.Traffic.RoadComponents.TrafficControl
{
    [DisallowMultipleComponent]
    public class TrafficControlBlockAuthoring : MonoBehaviour
    {
        [Header("Node Groups")]
        [SerializeField] public List<ControlGroup> _groups;

        [Header("State Masks")]
        [SerializeField] public List<ControlState> _stateMasks;

        [Header("Start Setup")]
        [SerializeField] public int _startStateId;

        [System.Serializable]
        public struct ControlGroup
        {
            public string groupName;
            public List<RoadNodeAuthoring> groupNodes;
        }

        [System.Serializable]
        public class ControlState
        {
            public List<bool> mask;
            public int stateLifetime;
        }
    }
}
