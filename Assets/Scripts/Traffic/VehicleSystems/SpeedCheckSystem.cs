using Core.Systems;
using Traffic.VehicleComponents;
using Traffic.VehicleComponents.DriveVehicle;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Traffic.VehicleSystems
{
    [UpdateInGroup(typeof(AfterProcessVehiclesSystemGroup))]
    [UpdateAfter(typeof(VehiclesEndProcessSystem))]
    [AlwaysSynchronizeSystem]
    public class SpeedCheckSystem : SystemBase
    {
        private BuildPhysicsWorld _buildPhysicsWorldSystem;
        private SystemWithPublicDependencyBase _lastSystem;

        protected override void OnCreate()
        {
            _buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
            _lastSystem = World.GetOrCreateSystem<VehiclesEndProcessSystem>();
        }

        protected override void OnUpdate()
        {
            var physicsWorld = _buildPhysicsWorldSystem.PhysicsWorld;
            
            Entities
                .WithReadOnly(physicsWorld)
                .WithAll<VehicleTag>()
                .ForEach((Entity vehicleEntity, ref VehicleEngineData engine, in LocalToWorld vehicleTransforms) =>
                {
                    var vehicleRbIndex = physicsWorld.GetRigidBodyIndex(vehicleEntity);
                    if (vehicleRbIndex == -1 || vehicleRbIndex >= physicsWorld.NumDynamicBodies)
                        return;

                    var dirForward = vehicleTransforms.Forward;
                    
                    var vehicleLinearVelocity = physicsWorld.GetLinearVelocity(vehicleRbIndex);
                    engine.currentSpeed = math.dot(vehicleLinearVelocity, dirForward);
                }).ScheduleParallel(_lastSystem.PublicDependency).Complete();
        }
    }
}