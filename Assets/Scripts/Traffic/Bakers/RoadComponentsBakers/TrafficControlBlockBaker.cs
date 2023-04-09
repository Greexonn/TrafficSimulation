using System.Linq;
using TrafficSimulation.Traffic.RoadComponents.TrafficControl;
using TrafficSimulation.Traffic.VehicleComponents;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Bakers.RoadComponentsBakers
{
    public class TrafficControlBlockBaker : Baker<TrafficControlBlockAuthoring>
    {
        public override void Bake(TrafficControlBlockAuthoring authoring)
        {
            //start check
            foreach (var state in authoring._stateMasks)
            {
                if (state.mask.Count != authoring._groups.Count)
                    throw new System.Exception("Traffic Control Mask value count is not equal to groups count");

                if (authoring._startStateId >= authoring._stateMasks.Count)
                    throw new System.Exception("Start state ID is out of bounds");
            }
            
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent<TrafficControlBlockInitTag>(entity);
            AddComponent(entity, new TrafficControlBlockComponent { groupsCount = authoring._groups.Count, statesCount = authoring._stateMasks.Count });
            AddComponent(entity, new TrafficControlStateComponent { stateId = authoring._startStateId, stateRemainingTime = authoring._stateMasks[authoring._startStateId].stateLifetime });
            
            //add groups
            var groupsBuffer = AddBuffer<NodeBufferElement>(entity);
            for (var i = 0; i < authoring._groups.Count; i++)
            {
                foreach (var node in authoring._groups[i].groupNodes)
                {
                    groupsBuffer.Add(new NodeBufferElement { Node = GetEntity(node.gameObject, TransformUsageFlags.Dynamic) });
                }
            }
            
            //add start IDs
            var groupStartIdsBuffer = AddBuffer<StartIDsBufferElement>(entity);
            var startCounter = 0;
            groupStartIdsBuffer.Add(new StartIDsBufferElement { Value = startCounter });
            for (var i = 0; i < authoring._groups.Count; i++)
            {
                startCounter += authoring._groups[i].groupNodes.Count;
                groupStartIdsBuffer.Add(new StartIDsBufferElement { Value = startCounter });

            }
            
            //add states
            var statesBuffer = AddBuffer<TCStateBufferElement>(entity);
            foreach (var t1 in authoring._stateMasks.SelectMany(stateMask => stateMask.mask))
            {
                statesBuffer.Add(new TCStateBufferElement { Value = t1 });
            }
            
            //add state timings
            var stateTimesBuffer = AddBuffer<StateTimeBufferElement>(entity);
            foreach (var stateMask in authoring._stateMasks)
            {
                stateTimesBuffer.Add(new StateTimeBufferElement { Value = stateMask.stateLifetime });
            }
        }
    }
}