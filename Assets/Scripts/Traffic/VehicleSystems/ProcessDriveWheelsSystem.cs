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
    [UpdateAfter(typeof(ProcessBrakesSystem))]
    public class ProcessDriveWheelsSystem : SystemWithPublicDependencyBase
    {
        private BuildPhysicsWorld _buildPhysicsWorldSystem;
        private SystemWithPublicDependencyBase _sidewaysFrictionSystem;

        protected override void OnCreate()
        {
            _buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
            _sidewaysFrictionSystem = World.GetOrCreateSystem<ProcessBrakesSystem>();
        }
        
        protected override void OnUpdate()
        {
            var physicsWorld = _buildPhysicsWorldSystem.PhysicsWorld;
            
            var localToWorldComponents = GetComponentDataFromEntity<LocalToWorld>(true);
            var vehicleEngineComponents = GetComponentDataFromEntity<VehicleEngineData>(true);
            
            var handle = Entities
                .WithReadOnly(localToWorldComponents)
                .WithReadOnly(vehicleEngineComponents)
                .WithAll<DriveWheelTag>()
                .ForEach((in WheelRaycastData raycastData, in VehicleRefData vehicleRef, in LocalToWorld wheelRoot, in WheelData wheelData) =>
                {
                    if (!raycastData.IsHitThisFrame)
                        return;
                    
                    var vehicleRbIndex = physicsWorld.GetRigidBodyIndex(vehicleRef.Entity);
                    if (vehicleRbIndex == -1 || vehicleRbIndex >= physicsWorld.NumDynamicBodies)
                        return;

                    var engine = vehicleEngineComponents[vehicleRef.Entity];
                    
                    var vehicleTransforms = localToWorldComponents[vehicleRef.Entity];
                    var dirForward = vehicleTransforms.Forward;
                    
                    var wheelForward = wheelRoot.Forward;
                    
                    var direction = math.dot(wheelForward, dirForward);
                    direction /= math.abs(direction);

                    var impulse = wheelForward * direction;
                    var impulseKoef = 1.0f - (engine.currentSpeed / engine.maxSpeed);
                    var effectiveMass =
                        physicsWorld.GetEffectiveMass(vehicleRbIndex, impulse, wheelData.wheelPosition);
                    var impulseValue = effectiveMass * wheelData.forwardFriction * impulseKoef *
                        engine.acceleration / 100;
                    impulseValue = math.clamp(impulseValue, -wheelData.maxForwardFriction,
                        wheelData.maxForwardFriction);

                    impulse *= impulseValue;

                    physicsWorld.ApplyImpulse(vehicleRbIndex, impulse, wheelData.wheelPosition);
                    physicsWorld.ApplyImpulse(raycastData.HitRBIndex, -impulse, raycastData.HitPosition);
                    
                }).ScheduleParallel(_sidewaysFrictionSystem.PublicDependency);
            
            Dependency = JobHandle.CombineDependencies(Dependency, handle);
        }
    }
}
