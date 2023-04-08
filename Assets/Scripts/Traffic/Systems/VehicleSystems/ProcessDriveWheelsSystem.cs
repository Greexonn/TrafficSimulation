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
    [UpdateAfter(typeof(ProcessBrakesSystem))]
    public partial class ProcessDriveWheelsSystem : SystemWithPublicDependencyBase
    {
        private SystemWithPublicDependencyBase _sidewaysFrictionSystem;

        protected override void OnCreate()
        {
            _sidewaysFrictionSystem = World.GetOrCreateSystemManaged<ProcessBrakesSystem>();
            RequireForUpdate<BuildPhysicsWorldData>();
        }
        
        protected override void OnUpdate()
        {
            var physicsWorld = SystemAPI.GetSingleton<BuildPhysicsWorldData>().PhysicsData.PhysicsWorld;

            var handle = Entities
                .WithAll<DriveWheelTag>()
                .ForEach((in WheelRaycastData raycastData, in VehicleRefData vehicleRef, in LocalToWorld wheelRoot, in WheelData wheelData) =>
                {
                    if (!raycastData.IsHitThisFrame)
                        return;
                    
                    var vehicleRbIndex = physicsWorld.GetRigidBodyIndex(vehicleRef.Entity);
                    if (vehicleRbIndex == -1 || vehicleRbIndex >= physicsWorld.NumDynamicBodies)
                        return;

                    var engine = SystemAPI.GetComponent<VehicleEngineData>(vehicleRef.Entity);

                    var vehicleTransforms = SystemAPI.GetComponent<LocalToWorld>(vehicleRef.Entity);
                    var dirForward = vehicleTransforms.Forward;
                    
                    var wheelForward = wheelRoot.Forward;
                    
                    var direction = math.dot(wheelForward, dirForward);
                    direction /= math.abs(direction);

                    var impulse = wheelForward * direction;
                    var impulseCoeff = 1.0f - engine.CurrentSpeed / engine.MaxSpeed;
                    var effectiveMass = physicsWorld.GetEffectiveMass(vehicleRbIndex, impulse, wheelData.wheelPosition);
                    var impulseValue = effectiveMass * wheelData.forwardFriction * impulseCoeff * engine.Acceleration / 100;
                    impulseValue = math.clamp(impulseValue, -wheelData.maxForwardFriction, wheelData.maxForwardFriction);

                    impulse *= impulseValue;

                    physicsWorld.ApplyImpulse(vehicleRbIndex, impulse, wheelData.wheelPosition);
                    physicsWorld.ApplyImpulse(raycastData.HitRBIndex, -impulse, raycastData.HitPosition);
                }).ScheduleParallel(_sidewaysFrictionSystem.PublicDependency);
            
            Dependency = JobHandle.CombineDependencies(Dependency, handle);
        }
    }
}
