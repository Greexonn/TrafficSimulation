using Traffic.VehicleComponents.Wheel;
using Unity.Entities;
using Unity.Transforms;

namespace Traffic.VehicleSystems
{
    [UpdateInGroup(typeof(VehiclesProcessUpdateSystemGroup))]
    public class UpdateWheelPositionSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _commandBufferSystem;

        protected override void OnCreate()
        {
            _commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var commandBuffer = _commandBufferSystem.CreateCommandBuffer();
            var parallelCommandBuffer = commandBuffer.AsParallelWriter();
            
            Entities
                .ForEach((int nativeThreadIndex, in WheelData wheelData, in LocalToWorld wheelRoot) =>
                {
                    var wheelLocalPos = wheelData.wheelPosition - wheelRoot.Position;
                    parallelCommandBuffer.SetComponent(nativeThreadIndex, wheelData.wheelModel,
                        new Translation {Value = wheelLocalPos});
                }).ScheduleParallel(Dependency).Complete();
        }
    }
}
