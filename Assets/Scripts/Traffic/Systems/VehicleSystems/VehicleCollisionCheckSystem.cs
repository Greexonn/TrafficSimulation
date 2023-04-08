using TrafficSimulation.Traffic.VehicleComponents;
using TrafficSimulation.Traffic.VehicleComponents.DriveVehicle;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace TrafficSimulation.Traffic.Systems.VehicleSystems
{
    [UpdateInGroup(typeof(PreprocessVehiclesSystemGroup))]
    [UpdateAfter(typeof(WheelsRaycastSystem))]
    [AlwaysSynchronizeSystem]
    public partial class VehicleCollisionCheckSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<BuildPhysicsWorldData>();
        }

        protected override void OnUpdate()
        {
            var physicsWorld = SystemAPI.GetSingleton<BuildPhysicsWorldData>().PhysicsData.PhysicsWorld;

            Entities
                .WithReadOnly(physicsWorld)
                .WithAll<VehicleTag>()
                .ForEach((ref VehicleBrakesData brakes, ref VehicleEngineData engine,
                    in VehicleAICollisionDetectionComponent collisionDetection, in LocalToWorld transforms,
                    in VehicleSteeringData steering) =>
                {
                    var rayDirection = math.forward(steering.CurrentRotation);
                    if (math.dot(rayDirection, transforms.Forward) < 0)
                        return;

                    var leftPos = SystemAPI.GetComponent<LocalToWorld>(collisionDetection.LeftRayPoint).Position;
                    var rightPos = SystemAPI.GetComponent<LocalToWorld>(collisionDetection.RightRayPoint).Position;

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
                    brakes.BrakesUsage = usage;
                    engine.Acceleration = 0;
                }).ScheduleParallel(Dependency).Complete();
        }
    }
}