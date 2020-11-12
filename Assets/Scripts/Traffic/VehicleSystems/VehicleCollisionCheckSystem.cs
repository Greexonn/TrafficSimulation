using Traffic.VehicleComponents;
using Traffic.VehicleComponents.DriveVehicle;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Traffic.VehicleSystems
{
    [UpdateInGroup(typeof(PreprocessVehiclesSystemGroup))]
    [UpdateAfter(typeof(WheelsRaycastSystem))]
    [AlwaysSynchronizeSystem]
    public class VehicleCollisionCheckSystem : SystemBase
    {
        private BuildPhysicsWorld _buildPhysicsWorldSystem;

        protected override void OnCreate()
        {
            _buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        }

        protected override void OnUpdate()
        {
            var physicsWorld = _buildPhysicsWorldSystem.PhysicsWorld;

            var localToWorldComponents = GetComponentDataFromEntity<LocalToWorld>(true);

            Entities
                .WithReadOnly(physicsWorld)
                .WithReadOnly(localToWorldComponents)
                .WithAll<VehicleTag>()
                .ForEach((Entity vehicleEntity, ref VehicleBrakesData brakes, ref VehicleEngineData engine, 
                    in VehicleAICollisionDetectionComponent collisionDetection, in LocalToWorld transforms, in VehicleSteeringData steering) =>
            {
                var rayDirection = math.forward(steering.currentRotation);
                if (math.dot(rayDirection, transforms.Forward) < 0)
                    return;

                var leftPos = localToWorldComponents[collisionDetection.leftRayPoint].Position;
                var rightPos = localToWorldComponents[collisionDetection.rightRayPoint].Position;

                var distance = math.clamp(engine.currentSpeed, 2, engine.currentSpeed);

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

                var isLeftHit = physicsWorld.CastRay(raycastInputLeft, out var hitLeft);
                var isRightHit = physicsWorld.CastRay(raycastInputRight, out var hitRight);

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
                brakes.brakesUsage = usage;
                engine.acceleration = 0;
            }).ScheduleParallel(Dependency).Complete();
        }
    }
}