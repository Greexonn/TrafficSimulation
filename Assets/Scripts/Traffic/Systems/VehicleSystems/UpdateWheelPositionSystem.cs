using TrafficSimulation.Traffic.VehicleComponents.Wheel;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace TrafficSimulation.Traffic.Systems.VehicleSystems
{
    [UpdateInGroup(typeof(VehiclesProcessUpdateSystemGroup))]
    public partial struct UpdateWheelPositionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var job = new UpdateWheelPositionsJob
            {
                LocalTransformLookup = GetComponentLookup<LocalTransform>()
            };
            job.ScheduleParallelByRef(state.Dependency);
        }
        
        [BurstCompile]
        private partial struct UpdateWheelPositionsJob : IJobEntity
        {
            public ComponentLookup<LocalTransform> LocalTransformLookup;

            private void Execute(in WheelData wheelData, in LocalToWorld wheelRoot)
            {
                var wheelLocalPos = wheelData.wheelPosition - wheelRoot.Position;
                var wheelModelTransformRef = LocalTransformLookup.GetRefRW(wheelData.wheelModel, false);
                wheelModelTransformRef.ValueRW.Position = wheelLocalPos;
            }
        }
    }
}
