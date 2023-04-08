using TrafficSimulation.Traffic.RoadComponents;
using TrafficSimulation.Traffic.RoadComponents.TrafficControl;
using TrafficSimulation.Traffic.VehicleComponents;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Systems.RoadSystems.TrafficControls
{
    public partial class TrafficControlUpdateSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        protected override void OnUpdate()
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            var parallelCommandBuffer = commandBuffer.AsParallelWriter();

            Entities
                .WithNone<TrafficControlBlockInitTag>()
                .ForEach((int nativeThreadIndex, ref TrafficControlBlockComponent controlBlock,
                    ref TrafficControlStateComponent controlState,
                    in DynamicBuffer<NodeBufferElement> groupsBuffer,
                    in DynamicBuffer<StartIDsBufferElement> groupStartIdsBuffer,
                    in DynamicBuffer<TCStateBufferElement> statesBuffer,
                    in DynamicBuffer<StateTimeBufferElement> stateTimesBuffer) =>
                {
                    //decrease state time
                    controlState.stateRemainingTime -= deltaTime;
                    //check time
                    if (!(controlState.stateRemainingTime <= 0))
                        return;

                    //change state
                    controlState.stateId++;
                    if (controlState.stateId >= statesBuffer.Length / controlBlock.groupsCount)
                        controlState.stateId = 0;

                    controlState.stateRemainingTime = stateTimesBuffer[controlState.stateId].Value;

                    //update groups
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
                            parallelCommandBuffer.SetComponent(nativeThreadIndex, groupsBuffer[g].Node, new RoadNodeData { IsOpen = groupState });
                        }
                    }
                }).ScheduleParallel(Dependency).Complete();
        }
    }
}