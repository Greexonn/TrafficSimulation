using TrafficSimulation.Core.Systems;
using TrafficSimulation.Traffic.VehicleComponents;
using TrafficSimulation.Traffic.VehicleComponents.DriveVehicle;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace TrafficSimulation.Traffic.Systems.VehicleSystems
{
    [UpdateInGroup(typeof(AfterProcessVehiclesSystemGroup))]
    [UpdateAfter(typeof(VehiclesEndProcessSystem))]
    [AlwaysSynchronizeSystem]
    public partial class SpeedCheckSystem : SystemBase
    {
        private SystemWithPublicDependencyBase _lastSystem;

        protected override void OnCreate()
        {
            _lastSystem = World.GetOrCreateSystemManaged<VehiclesEndProcessSystem>();
            RequireForUpdate<BuildPhysicsWorldData>();
        }

        protected override void OnUpdate()
        {
            var physicsWorld = SystemAPI.GetSingleton<BuildPhysicsWorldData>().PhysicsData.PhysicsWorld;
            
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
                    engine.CurrentSpeed = math.dot(vehicleLinearVelocity, dirForward);
                }).ScheduleParallel(_lastSystem.PublicDependency).Complete();
        }
    }
}