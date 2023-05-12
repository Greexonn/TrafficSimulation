using TrafficSimulation.Traffic.VehicleComponents.Wheel;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace TrafficSimulation.Traffic.Systems.VehicleSystems
{
    [UpdateInGroup(typeof(VehiclesProcessUpdateSystemGroup))]
    public partial struct UpdateWheelsPositionsSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate(new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<WheelData, LocalToWorld>()
                .Build(ref state));
        }

        public void OnUpdate(ref SystemState state)
        {
            var job = new UpdateWheelPositionsJob
            {
                LocalTransformLookup = GetComponentLookup<LocalTransform>()
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
        
        [BurstCompile]
        private partial struct UpdateWheelPositionsJob : IJobEntity
        {
            [NativeDisableContainerSafetyRestriction]  public ComponentLookup<LocalTransform> LocalTransformLookup;

            private void Execute(in WheelData wheelData, in LocalToWorld wheelRoot)
            {
                var wheelLocalPos = wheelData.wheelPosition - wheelRoot.Position;
                var wheelModelTransformRef = LocalTransformLookup.GetRefRW(wheelData.wheelModel);
                wheelModelTransformRef.ValueRW.Position = wheelLocalPos;
            }
        }
    }
}
