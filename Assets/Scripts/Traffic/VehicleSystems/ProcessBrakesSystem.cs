using Core.Systems;
using Traffic.VehicleComponents.DriveVehicle;
using Traffic.VehicleComponents.Wheel;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Traffic.VehicleSystems
{
    [UpdateInGroup(typeof(ProcessVehiclesSystemGroup))]
    [UpdateAfter(typeof(ProcessSidewaysFrictionSystem))]
    public class ProcessBrakesSystem : SystemWithPublicDependencyBase
    {
        private BuildPhysicsWorld _buildPhysicsWorldSystem;
        private SystemWithPublicDependencyBase _sidewaysFrictionSystem;

        protected override void OnCreate()
        {
            _buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
            _sidewaysFrictionSystem = World.GetOrCreateSystem<ProcessSidewaysFrictionSystem>();
        }
        
        protected override void OnUpdate()
        {
            var physicsWorld = _buildPhysicsWorldSystem.PhysicsWorld;

            var brakesComponents = GetComponentDataFromEntity<VehicleBrakesData>(true);
            
            var handle = Entities
                .WithReadOnly(brakesComponents)
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

                    var brakes = brakesComponents[vehicleRef.Entity];
                    
                    var impulseValue = -velocityForward * wheelData.forwardFriction;
                    var impulse = wheelForward * impulseValue;
                    var effectiveMass =
                        physicsWorld.GetEffectiveMass(vehicleRbIndex, impulse, raycastData.HitPosition);
                    impulseValue *= effectiveMass * brakes.brakesUsage / 100;
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
