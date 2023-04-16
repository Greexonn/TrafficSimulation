using TrafficSimulation.Traffic.VehicleComponents;
using TrafficSimulation.Traffic.VehicleComponents.DriveVehicle;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace TrafficSimulation.Traffic.Systems.VehicleSystems
{
    [UpdateInGroup(typeof(AfterProcessVehiclesSystemGroup))]
    [UpdateAfter(typeof(ProcessDriveWheelsSystem))]
    public partial struct SpeedCheckSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate(new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<VehicleTag, VehicleEngineData, LocalToWorld>()
                .Build(ref state));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var physicsSingleton = GetSingleton<PhysicsWorldSingleton>();

            var job = new SpeedCheckJob
            {
                PhysicsWorldSingleton = physicsSingleton
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
        
        [BurstCompile]
        [WithAll(typeof(VehicleTag))]
        private partial struct SpeedCheckJob : IJobEntity
        {
            [NativeDisableContainerSafetyRestriction] public PhysicsWorldSingleton PhysicsWorldSingleton;
            
            private void Execute(Entity vehicleEntity, ref VehicleEngineData engine, in LocalToWorld vehicleTransforms)
            {
                var vehicleRbIndex = PhysicsWorldSingleton.GetRigidBodyIndex(vehicleEntity);
                if (vehicleRbIndex == -1 || vehicleRbIndex >= PhysicsWorldSingleton.NumDynamicBodies)
                    return;

                var dirForward = vehicleTransforms.Forward;
                    
                var vehicleLinearVelocity = PhysicsWorldSingleton.PhysicsWorld.GetLinearVelocity(vehicleRbIndex);
                engine.CurrentSpeed = math.dot(vehicleLinearVelocity, dirForward);
            }
        }
    }
}