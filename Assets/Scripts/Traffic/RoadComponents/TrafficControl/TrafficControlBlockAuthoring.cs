using System.Collections.Generic;
using System.Linq;
using TrafficSimulation.Traffic.VehicleComponents;
using Unity.Entities;
using UnityEngine;

namespace TrafficSimulation.Traffic.RoadComponents.TrafficControl
{
    [DisallowMultipleComponent]
    public class TrafficControlBlockAuthoring : MonoBehaviour
    {
        [Header("Node Groups")]
        [SerializeField] private List<ControlGroup> _groups;

        [Header("State Masks")]
        [SerializeField] private List<ControlState> _stateMasks;

        [Header("Start Setup")]
        [SerializeField] private int _startStateId;

        public void Convert(Entity entity, EntityManager dstManager)
        {
            //start check
            foreach (var state in _stateMasks)
            {
                if (state.mask.Count != _groups.Count)
                    throw new System.Exception("Traffic Control Mask value count is not equal to groups count");

                if (_startStateId >= _stateMasks.Count)
                    throw new System.Exception("Start state ID is out of bounds");
            }

            //convert
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            //add components
            manager.AddComponent(entity, typeof(TrafficControlBlockInitTag));
            manager.AddComponentData(entity, new TrafficControlBlockComponent { groupsCount = _groups.Count, statesCount = _stateMasks.Count });
            manager.AddComponentData(entity, new TrafficControlStateComponent { stateId = _startStateId, stateRemainingTime = _stateMasks[_startStateId].stateLifetime });


            //add groups
            var groupsBuffer = manager.AddBuffer<NodeBufferElement>(entity);
            for (var i = 0; i < _groups.Count; i++)
            {
                foreach (var node in _groups[i].groupNodes)
                {
                    // groupsBuffer.Add(new NodeBufferElement { node = conversionSystem.GetPrimaryEntity(node.gameObject) });
                }
            }
            //add start IDs
            var groupStartIdsBuffer = manager.AddBuffer<StartIDsBufferElement>(entity);
            var startCounter = 0;
            groupStartIdsBuffer.Add(new StartIDsBufferElement { Value = startCounter });
            for (var i = 0; i < _groups.Count; i++)
            {
                startCounter += _groups[i].groupNodes.Count;
                groupStartIdsBuffer.Add(new StartIDsBufferElement { Value = startCounter });

            }
            //add states
            var statesBuffer = manager.AddBuffer<TCStateBufferElement>(entity);
            foreach (var t1 in _stateMasks.SelectMany(stateMask => stateMask.mask))
            {
                statesBuffer.Add(new TCStateBufferElement { Value = t1 });
            }
            //add state timings
            var stateTimesBuffer = manager.AddBuffer<StateTimeBufferElement>(entity);
            foreach (var stateMask in _stateMasks)
            {
                stateTimesBuffer.Add(new StateTimeBufferElement { Value = stateMask.stateLifetime });
            }
        }

        [System.Serializable]
        private struct ControlGroup
        {
            public string groupName;
            public List<RoadNodeAuthoring> groupNodes;
        }

        [System.Serializable]
        private class ControlState
        {
            public List<bool> mask;
            public int stateLifetime;
        }
    }
}
