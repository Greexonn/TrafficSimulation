using TrafficSimulation.Traffic.VehicleComponents;
using TrafficSimulation.Traffic.VehicleComponents.DriveVehicle;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace TrafficSimulation.Traffic.Systems.VehicleSystems
{
    [UpdateInGroup(typeof(PreprocessVehiclesSystemGroup))]
    [UpdateAfter(typeof(WheelsRaycastSystem))]
    public partial struct VehiclesCollisionsCheckSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var physicsSingleton = GetSingleton<PhysicsWorldSingleton>();

            var job = new VehiclesCollisionsCheckJob
            {
                LocalToWorldLookup = GetComponentLookup<LocalToWorld>(true),
                PhysicsSingleton = physicsSingleton
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(VehicleTag))]
        private partial struct VehiclesCollisionsCheckJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;
            [ReadOnly] public PhysicsWorldSingleton PhysicsSingleton;

            private void Execute(ref VehicleBrakesData brakes, ref VehicleEngineData engine, in VehicleAICollisionDetectionComponent collisionDetection, in LocalToWorld transforms, in VehicleSteeringData steering)
            {
                var rayDirection = math.forward(steering.CurrentRotation);
                if (math.dot(rayDirection, transforms.Forward) < 0)
                    return;

                var leftPos = LocalToWorldLookup[collisionDetection.LeftRayPoint].Position;
                var rightPos = LocalToWorldLookup[collisionDetection.RightRayPoint].Position;

                var distance = math.clamp(engine.CurrentSpeed, 2, engine.CurrentSpeed);

                var ray = rayDirection * distance;

                //cast rays
                var raycastInputLeft = new RaycastInput
                {
                    Start = leftPos,
                    End = leftPos + ray,
                    Filter = CollisionFilter.Default
                };
                var raycastInputRight = new RaycastInput
                {
                    Start = rightPos,
                    End = rightPos + ray,
                    Filter = CollisionFilter.Default
                };

                var isLeftHit = PhysicsSingleton.CastRay(raycastInputLeft, out var hitLeft);
                var isRightHit = PhysicsSingleton.CastRay(raycastInputRight, out var hitRight);

                float fraction;

                if (isLeftHit && isRightHit)
                {
                    fraction = hitLeft.Fraction;
                    if (hitRight.Fraction < fraction)
                        fraction = hitRight.Fraction;

                    //debug
                    // DrawLine(raycastInputLeft.Start, hitLeft.Position, UnityEngine.Color.red);
                    // DrawLine(raycastInputRight.Start, hitRight.Position, UnityEngine.Color.red);
                }
                else if (isLeftHit)
                {
                    fraction = hitLeft.Fraction;

                    //debug
                    // DrawLine(raycastInputLeft.Start, hitLeft.Position, UnityEngine.Color.red);
                }
                else if (isRightHit)
                {
                    fraction = hitRight.Fraction;

                    //debug
                    // DrawLine(raycastInputRight.Start, hitRight.Position, UnityEngine.Color.red);
                }
                else
                {
                    return;
                }

                //set brakes
                var closeValue = 1.0f - fraction;
                var usage = (int)(closeValue * 100);
                brakes.BrakesUsage = usage;
                engine.Acceleration = 0;
            }
        }
    }
}