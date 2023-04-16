using TrafficSimulation.Traffic.VehicleComponents.Wheel;
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
    [UpdateInGroup(typeof(ProcessVehiclesSystemGroup))]
    [UpdateAfter(typeof(ProcessSuspensionSystem))]
    public partial struct ProcessSidewaysFrictionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate(new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<WheelRaycastData, VehicleRefData, LocalToWorld, WheelData>()
                .Build(ref state));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var physicsSingleton = GetSingleton<PhysicsWorldSingleton>();

            var job = new ProcessSidewaysFrictionJob
            {
                PhysicsWorldSingleton = physicsSingleton
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
        
        [BurstCompile]
        private partial struct ProcessSidewaysFrictionJob : IJobEntity
        {
            [NativeDisableContainerSafetyRestriction] public PhysicsWorldSingleton PhysicsWorldSingleton;
            
            private void Execute(in WheelRaycastData raycastData, in VehicleRefData vehicleRef, in LocalToWorld wheelRoot, in WheelData wheelData)
            {
                if (!raycastData.IsHitThisFrame)
                    return;

                var vehicleRbIndex = PhysicsWorldSingleton.GetRigidBodyIndex(vehicleRef.Entity);
                if (vehicleRbIndex == -1 || vehicleRbIndex >= PhysicsWorldSingleton.NumDynamicBodies)
                    return;

                var wheelRight = wheelRoot.Right;

                var currentSpeedRight = math.dot(raycastData.VelocityAtWheel, wheelRight);

                var impulseValue = -currentSpeedRight * wheelData.sideFriction;
                var impulse = impulseValue * wheelRight;

                var effectiveMass = PhysicsWorldSingleton.PhysicsWorld.GetEffectiveMass(vehicleRbIndex, impulse, raycastData.HitPosition) / vehicleRef.WheelsCount;
                impulseValue *= effectiveMass;

                impulseValue = math.clamp(impulseValue, -wheelData.maxSideFriction, wheelData.maxSideFriction);
                impulse = impulseValue * wheelRight;

                PhysicsWorldSingleton.PhysicsWorld.ApplyImpulse(vehicleRbIndex, impulse, raycastData.HitPosition);
                PhysicsWorldSingleton.PhysicsWorld.ApplyImpulse(raycastData.HitRBIndex, -impulse, raycastData.HitPosition);
            }
        }
    }
}
