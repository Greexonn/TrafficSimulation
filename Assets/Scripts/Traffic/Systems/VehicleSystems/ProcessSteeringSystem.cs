using TrafficSimulation.Traffic.VehicleComponents.DriveVehicle;
using TrafficSimulation.Traffic.VehicleComponents.Wheel;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace TrafficSimulation.Traffic.Systems.VehicleSystems
{
    [UpdateInGroup(typeof(VehiclesProcessUpdateSystemGroup))]
    public partial struct ProcessSteeringSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate(new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<ControlWheelTag, LocalTransform, VehicleRefData>()
                .Build(ref state));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new ProcessSteeringJob
            {
                VehicleSteeringDataLookup = GetComponentLookup<VehicleSteeringData>(true)
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
        
        [BurstCompile]
        [WithAll(typeof(ControlWheelTag))]
        private partial struct ProcessSteeringJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<VehicleSteeringData> VehicleSteeringDataLookup;

            private void Execute(ref LocalTransform wheelTransform, in VehicleRefData vehicleRef)
            {
                var steeringData = VehicleSteeringDataLookup[vehicleRef.Entity];
                    
                wheelTransform.Rotation = steeringData.CurrentRotation;
            }
        }
    }
}
