using TrafficSimulation.Core.Systems;
using TrafficSimulation.Traffic.VehicleComponents.DriveVehicle;
using TrafficSimulation.Traffic.VehicleComponents.Wheel;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace TrafficSimulation.Traffic.Systems.VehicleSystems
{
    [UpdateInGroup(typeof(ProcessVehiclesSystemGroup))]
    [UpdateAfter(typeof(ProcessSidewaysFrictionSystem))]
    public partial class ProcessBrakesSystem : SystemWithPublicDependencyBase
    {
        private SystemWithPublicDependencyBase _sidewaysFrictionSystem;

        protected override void OnCreate()
        {
            _sidewaysFrictionSystem = World.GetOrCreateSystemManaged<ProcessSidewaysFrictionSystem>();
            RequireForUpdate<BuildPhysicsWorldData>();
        }
        
        protected override void OnUpdate()
        {
            var physicsWorld = SystemAPI.GetSingleton<BuildPhysicsWorldData>().PhysicsData.PhysicsWorld;

            var handle = Entities
                .WithAll<BrakeWheelTag>()
                .ForEach((in WheelRaycastData raycastData, in VehicleRefData vehicleRef, in LocalToWorld wheelRoot, in WheelData wheelData) =>
                {
                    if (!raycastData.IsHitThisFrame)
                        return;
                    
                    var vehicleRbIndex = physicsWorld.GetRigidBodyIndex(vehicleRef.Entity);
                    if (vehicleRbIndex == -1 || vehicleRbIndex >= physicsWorld.NumDynamicBodies)
                        return;
                    
                    var wheelForward = wheelRoot.Forward;
                    var velocityForward = math.dot(raycastData.VelocityAtWheel, wheelForward);

                    var brakes = SystemAPI.GetComponent<VehicleBrakesData>(vehicleRef.Entity);
                    
                    var impulseValue = -velocityForward * wheelData.forwardFriction;
                    var impulse = wheelForward * impulseValue;
                    var effectiveMass = physicsWorld.GetEffectiveMass(vehicleRbIndex, impulse, raycastData.HitPosition);
                    impulseValue *= effectiveMass * brakes.BrakesUsage / 100;
                    impulseValue = math.clamp(impulseValue, -wheelData.maxForwardFriction,
                        wheelData.maxForwardFriction);

                    impulse = wheelForward * impulseValue;

                    physicsWorld.ApplyImpulse(vehicleRbIndex, impulse, raycastData.HitPosition);
                    physicsWorld.ApplyImpulse(raycastData.HitRBIndex, -impulse, raycastData.HitPosition);
                }).ScheduleParallel(_sidewaysFrictionSystem.PublicDependency);
            
            Dependency = JobHandle.CombineDependencies(Dependency, handle);
        }
    }
}
