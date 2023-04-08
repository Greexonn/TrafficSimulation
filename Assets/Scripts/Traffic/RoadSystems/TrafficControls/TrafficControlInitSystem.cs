using Traffic.RoadComponents;
using Traffic.RoadComponents.TrafficControl;
using Unity.Collections;
using Unity.Entities;

namespace Traffic.RoadSystems.TrafficControls
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
                        var groupState = statesBuffer[stateOffset + i].value;
                        //apply state to group
                        var groupStartId = groupStartIdsBuffer[i].value;
                        var groupEndId = groupStartIdsBuffer[i + 1].value;
                        for (var g = groupStartId; g < groupEndId; g++)
                        {
                            parallelCommandBuffer.SetComponent(nativeThreadIndex, groupsBuffer[g].node,
                                new RoadNodeData { isOpen = groupState });
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