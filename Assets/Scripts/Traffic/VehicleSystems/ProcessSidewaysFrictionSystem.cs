using Core.Systems;
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
    [UpdateAfter(typeof(ProcessSuspensionSystem))]
    public partial class ProcessSidewaysFrictionSystem : SystemWithPublicDependencyBase
    {
        private SystemWithPublicDependencyBase _suspensionSystem;

        protected override void OnCreate()
        {
            _suspensionSystem = World.GetOrCreateSystemManaged<ProcessSuspensionSystem>();
        }
        
        protected override void OnUpdate()
        {
            var physicsWorld = SystemAPI.GetSingleton<BuildPhysicsWorldData>().PhysicsData.PhysicsWorld;
            
            var handle = Entities
                .ForEach((in WheelRaycastData raycastData, in VehicleRefData vehicleRef, in LocalToWorld wheelRoot, in WheelData wheelData) =>
                {
                    if (!raycastData.IsHitThisFrame)
                        return;
                    
                    var vehicleRbIndex = physicsWorld.GetRigidBodyIndex(vehicleRef.Entity);
                    if (vehicleRbIndex == -1 || vehicleRbIndex >= physicsWorld.NumDynamicBodies)
                        return;
                    
                    var wheelRight = wheelRoot.Right;
                    
                    var currentSpeedRight = math.dot(raycastData.VelocityAtWheel, wheelRight);
                    
                    var impulseValue = -currentSpeedRight * wheelData.sideFriction;
                    var impulse = impulseValue * wheelRight;
                    
                    var effectiveMass = physicsWorld.GetEffectiveMass(vehicleRbIndex, impulse, raycastData.HitPosition) / vehicleRef.WheelsCount;
                    impulseValue *= effectiveMass;
                    
                    impulseValue = math.clamp(impulseValue, -wheelData.maxSideFriction,
                        wheelData.maxSideFriction);
                    impulse = impulseValue * wheelRight;
                    
                    physicsWorld.ApplyImpulse(vehicleRbIndex, impulse, raycastData.HitPosition);
                    physicsWorld.ApplyImpulse(raycastData.HitRBIndex, -impulse, raycastData.HitPosition);
                }).ScheduleParallel(_suspensionSystem.PublicDependency);
            
            Dependency = JobHandle.CombineDependencies(Dependency, handle);
        }
    }
}
