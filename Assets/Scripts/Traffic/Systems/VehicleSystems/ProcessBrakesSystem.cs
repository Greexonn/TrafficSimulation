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
    [UpdateAfter(typeof(ProcessSidewaysFrictionSystem))]
    public partial struct ProcessBrakesSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate(new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<BrakeWheelTag, WheelRaycastData, VehicleRefData, LocalToWorld, WheelData>()
                .Build(ref state));
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var physicsSingleton = GetSingleton<PhysicsWorldSingleton>();

            var job = new ProcessBrakesJob
            {
                VehicleBrakesDataLookup = GetComponentLookup<VehicleBrakesData>(true),
                PhysicsWorldSingleton = physicsSingleton
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
        
        [BurstCompile]
        [WithAll(typeof(BrakeWheelTag))]
        private partial struct ProcessBrakesJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<VehicleBrakesData> VehicleBrakesDataLookup;
            
            [NativeDisableContainerSafetyRestriction] public PhysicsWorldSingleton PhysicsWorldSingleton;

            private void Execute(in WheelRaycastData raycastData, in VehicleRefData vehicleRef, in LocalToWorld wheelRoot, in WheelData wheelData)
            {
                if (!raycastData.IsHitThisFrame)
                    return;
                    
                var vehicleRbIndex = PhysicsWorldSingleton.GetRigidBodyIndex(vehicleRef.Entity);
                if (vehicleRbIndex == -1 || vehicleRbIndex >= PhysicsWorldSingleton.NumDynamicBodies)
                    return;
                    
                var wheelForward = wheelRoot.Forward;
                var velocityForward = math.dot(raycastData.VelocityAtWheel, wheelForward);

                var brakes = VehicleBrakesDataLookup[vehicleRef.Entity];
                    
                var impulseValue = -velocityForward * wheelData.forwardFriction;
                var impulse = wheelForward * impulseValue;
                var effectiveMass = PhysicsWorldSingleton.PhysicsWorld.GetEffectiveMass(vehicleRbIndex, impulse, raycastData.HitPosition);
                impulseValue *= effectiveMass * brakes.BrakesUsage / 100f;
                impulseValue = math.clamp(impulseValue, -wheelData.maxForwardFriction, wheelData.maxForwardFriction);

                impulse = wheelForward * impulseValue;

                PhysicsWorldSingleton.PhysicsWorld.ApplyImpulse(vehicleRbIndex, impulse, raycastData.HitPosition);
                PhysicsWorldSingleton.PhysicsWorld.ApplyImpulse(raycastData.HitRBIndex, -impulse, raycastData.HitPosition);
            }
        }
    }
}
