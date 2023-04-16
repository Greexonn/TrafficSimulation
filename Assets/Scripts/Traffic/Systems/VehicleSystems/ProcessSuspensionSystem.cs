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
    public partial struct ProcessSuspensionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate(new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<WheelData, WheelRaycastData, SuspensionData, LocalToWorld, VehicleRefData>()
                .Build(ref state));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var physicsSingleton = GetSingleton<PhysicsWorldSingleton>();

            var job = new ProcessSuspensionJob
            {
                LocalToWorldLookup = GetComponentLookup<LocalToWorld>(true),
                DeltaTime = Time.fixedDeltaTime,
                PhysicsWorldSingleton = physicsSingleton
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
        
        [BurstCompile]
        private partial struct ProcessSuspensionJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;
            public float DeltaTime;

            [NativeDisableContainerSafetyRestriction] public PhysicsWorldSingleton PhysicsWorldSingleton;

            private void Execute(ref WheelData wheelData, in WheelRaycastData raycastData, in VehicleRefData vehicleRef, in SuspensionData suspensionData, in LocalToWorld wheelRoot)
            {
                var vehicleRbIndex = PhysicsWorldSingleton.GetRigidBodyIndex(vehicleRef.Entity);
                if (vehicleRbIndex == -1 || vehicleRbIndex >= PhysicsWorldSingleton.NumDynamicBodies)
                    return;


                float3 wheelPos;
                var suspensionTop = wheelRoot.Position;

                var vehicleTransforms = LocalToWorldLookup[vehicleRef.Entity];
                var dirUp = vehicleTransforms.Up;

                if (raycastData.IsHitThisFrame)
                {
                    // calculate wheel position
                    wheelPos = raycastData.HitPosition + dirUp * wheelData.radius;

                    //get vehicle up speed
                    var vehicleUpAtWheel = PhysicsWorldSingleton.PhysicsWorld.GetLinearVelocity(vehicleRbIndex, suspensionTop);
                    var speedUp = math.dot(vehicleUpAtWheel, dirUp);
                    //get spring compression'
                    var suspensionCurrentLength = math.length(wheelPos - suspensionTop);
                    var compression = 1.0f - suspensionCurrentLength / suspensionData.suspensionLength;

                    var impulseValue = compression * suspensionData.springStrength - suspensionData.damperStrength * speedUp / 10;
                    if (impulseValue > 0)
                    {
                        var impulse = dirUp * impulseValue;

                        PhysicsWorldSingleton.PhysicsWorld.ApplyImpulse(vehicleRbIndex, impulse, suspensionTop);
                        PhysicsWorldSingleton.PhysicsWorld.ApplyImpulse(raycastData.HitRBIndex, -impulse, raycastData.HitPosition);
                    }
                }
                else
                {
                    var wheelDesiredPos = suspensionTop - dirUp * suspensionData.suspensionLength;
                    var height = math.dot(wheelData.wheelPosition - suspensionTop, dirUp);
                    height = math.abs(height);
                    var fraction = height / suspensionData.suspensionLength;
                    fraction += suspensionData.damperStrength / suspensionData.springStrength * DeltaTime * 2;
                    fraction = math.clamp(fraction, 0, 1);
                    wheelPos = math.lerp(suspensionTop, wheelDesiredPos, fraction);
                }

                wheelData.wheelPosition = wheelPos;
            }
        }
    }
}
