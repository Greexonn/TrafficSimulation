using TrafficSimulation.Traffic.RoadComponents;
using TrafficSimulation.Traffic.RoadComponents.TrafficControl;
using TrafficSimulation.Traffic.VehicleComponents;
using Unity.Collections;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Systems.RoadSystems.TrafficControls
{
    public partial class TrafficControlInitSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            var parallelCommandBuffer = commandBuffer.AsParallelWriter();

            Entities
                .WithAll<TrafficControlBlockInitTag>()
                .ForEach((int nativeThreadIndex, Entity blockEntity, ref TrafficControlBlockComponent controlBlock,
                    ref TrafficControlStateComponent controlState,
                    in DynamicBuffer<NodeBufferElement> groupsBuffer,
                    in DynamicBuffer<StartIDsBufferElement> groupStartIdsBuffer,
                    in DynamicBuffer<TCStateBufferElement> statesBuffer) =>
                {
                    var stateId = controlState.stateId;
                    var stateOffset = stateId * controlBlock.groupsCount;

                    for (var i = 0; i < controlBlock.groupsCount; i++)
                    {
                        var groupState = statesBuffer[stateOffset + i].Value;
                        //apply state to group
                        var groupStartId = groupStartIdsBuffer[i].Value;
                        var groupEndId = groupStartIdsBuffer[i + 1].Value;
                        for (var g = groupStartId; g < groupEndId; g++)
                        {
                            parallelCommandBuffer.SetComponent(nativeThreadIndex, groupsBuffer[g].Node,
                                new RoadNodeData { IsOpen = groupState });
                        }
                    }

                    //remove init component
                    parallelCommandBuffer.RemoveComponent<TrafficControlBlockInitTag>(nativeThreadIndex, blockEntity);
                }).ScheduleParallel(Dependency).Complete();
            
            commandBuffer.Playback(EntityManager);
            commandBuffer.Dispose();
        }
    }
}