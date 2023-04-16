using TrafficSimulation.Traffic.VehicleComponents.DriveVehicle;
using TrafficSimulation.Traffic.VehicleComponents.Wheel;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace TrafficSimulation.Traffic.Systems.VehicleSystems
{
    [UpdateInGroup(typeof(ProcessVehiclesSystemGroup))]
    [UpdateAfter(typeof(ProcessBrakesSystem))]
    public partial struct ProcessDriveWheelsSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate(new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<DriveWheelTag, WheelRaycastData, VehicleRefData, LocalToWorld, WheelData>()
                .Build(ref state));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var physicsSingleton = GetSingleton<PhysicsWorldSingleton>();

            var job = new ProcessDriveWheelsJob
            {
                VehicleEngineDataLookup = GetComponentLookup<VehicleEngineData>(true),
                LocalToWorldLookup = GetComponentLookup<LocalToWorld>(true),
                PhysicsWorldSingleton = physicsSingleton
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
        
        [BurstCompile]
        [WithAll(typeof(DriveWheelTag))]
        private partial struct ProcessDriveWheelsJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<VehicleEngineData> VehicleEngineDataLookup;
            [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;
            
            [NativeDisableContainerSafetyRestriction] public PhysicsWorldSingleton PhysicsWorldSingleton;
            
            private void Execute(in WheelRaycastData raycastData, in VehicleRefData vehicleRef, in LocalToWorld wheelRoot, in WheelData wheelData)
            {
                if (!raycastData.IsHitThisFrame)
                    return;
                    
                var vehicleRbIndex = PhysicsWorldSingleton.GetRigidBodyIndex(vehicleRef.Entity);
                if (vehicleRbIndex == -1 || vehicleRbIndex >= PhysicsWorldSingleton.NumDynamicBodies)
                    return;

                var engine = VehicleEngineDataLookup[vehicleRef.Entity];

                var vehicleTransforms = LocalToWorldLookup[vehicleRef.Entity];
                var dirForward = vehicleTransforms.Forward;
                    
                var wheelForward = wheelRoot.Forward;
                    
                var direction = math.dot(wheelForward, dirForward);
                direction /= math.abs(direction);

                var impulse = wheelForward * direction;
                var impulseCoeff = 1.0f - engine.CurrentSpeed / engine.MaxSpeed;
                var effectiveMass = PhysicsWorldSingleton.PhysicsWorld.GetEffectiveMass(vehicleRbIndex, impulse, wheelData.wheelPosition);
                var impulseValue = effectiveMass * wheelData.forwardFriction * impulseCoeff * engine.Acceleration / 100;
                impulseValue = math.clamp(impulseValue, -wheelData.maxForwardFriction, wheelData.maxForwardFriction);

                impulse *= impulseValue;

                PhysicsWorldSingleton.PhysicsWorld.ApplyImpulse(vehicleRbIndex, impulse, wheelData.wheelPosition);
                PhysicsWorldSingleton.PhysicsWorld.ApplyImpulse(raycastData.HitRBIndex, -impulse, raycastData.HitPosition);
            }
        }
    }
}
