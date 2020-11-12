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
    [AlwaysSynchronizeSystem]
    public class ProcessSuspensionSystem : SystemWithPublicDependencyBase
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
            
            const float deltaTime = 1.0f / 60.0f;
            
            var handle = Entities
                .WithReadOnly(localToWorldComponents)
                .ForEach((ref WheelData wheelData, in WheelRaycastData raycastData, in VehicleRefData vehicleRef, 
                    in SuspensionData suspensionData, in LocalToWorld wheelRoot) =>
                {
                    var vehicleRbIndex = physicsWorld.GetRigidBodyIndex(vehicleRef.Entity);
                    if (vehicleRbIndex == -1 || vehicleRbIndex >= physicsWorld.NumDynamicBodies)
                        return;


                    float3 wheelPos;
                    var suspensionTop = wheelRoot.Position;
                    
                    var vehicleTransforms = localToWorldComponents[vehicleRef.Entity];
                    var dirUp = vehicleTransforms.Up;

                    if (raycastData.IsHitThisFrame)
                    {
                        // calculate wheel position
                        wheelPos = raycastData.HitPosition + dirUp * wheelData.radius;
                        
                        //get vehicle up speed
                        var vehicleUpAtWheel = physicsWorld.GetLinearVelocity(vehicleRbIndex, suspensionTop);
                        var speedUp = math.dot(vehicleUpAtWheel, dirUp);
                        //get spring compression'
                        var suspensionCurrentLength = math.length(wheelPos - suspensionTop);

                        var compression =
                            1.0f - suspensionCurrentLength / suspensionData.suspensionLength;

                        var impulseValue = compression * suspensionData.springStrength -
                                           suspensionData.damperStrength * speedUp / 10;

                        if (impulseValue > 0)
                        {
                            var impulse = dirUp * impulseValue;

                            physicsWorld.ApplyImpulse(vehicleRbIndex, impulse, suspensionTop);
                            physicsWorld.ApplyImpulse(raycastData.HitRBIndex, -impulse, raycastData.HitPosition);
                        }
                    }
                    else
                    {
                        var wheelDesiredPos = suspensionTop - dirUp * suspensionData.suspensionLength;
                        var height = math.dot((wheelData.wheelPosition - suspensionTop), dirUp);
                        height = math.abs(height);
                        var fraction = height / suspensionData.suspensionLength;
                        fraction += suspensionData.damperStrength / suspensionData.springStrength *
                                    deltaTime * 2;
                        fraction = math.clamp(fraction, 0, 1);
                        wheelPos = math.lerp(suspensionTop, wheelDesiredPos, fraction);
                    }
                    
                    wheelData.wheelPosition = wheelPos;

                }).ScheduleParallel(_buildPhysicsWorldSystem.GetOutputDependency());

            Dependency = JobHandle.CombineDependencies(Dependency, handle);
        }
    }
}
