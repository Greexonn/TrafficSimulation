using Traffic.VehicleComponents.Wheel;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Traffic.VehicleSystems
{
    [UpdateInGroup(typeof(PreprocessVehiclesSystemGroup))]
    public partial class WheelsRaycastSystem : SystemBase
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
                .WithAll<LocalToWorld>()
                .ForEach((Entity wheelEntity, ref WheelRaycastData raycastData, in WheelData wheelData, in SuspensionData suspensionData, 
                    in LocalToWorld wheelRoot, in VehicleRefData vehicleRef) =>
                {
                    var vehicleRbIndex = physicsWorld.GetRigidBodyIndex(vehicleRef.Entity);
                    if (vehicleRbIndex == -1 || vehicleRbIndex >= physicsWorld.NumDynamicBodies)
                        return;
                    
                    var suspensionTop = wheelRoot.Position;

                    var vehicleTransforms = SystemAPI.GetComponent<LocalToWorld>(vehicleRef.Entity);
                    var dirUp = vehicleTransforms.Up;

                    var filter = physicsWorld.GetCollisionFilter(vehicleRbIndex);
                    var raycastInput = new RaycastInput
                    {
                        Start = suspensionTop,
                        End = suspensionTop -
                              dirUp * (wheelData.radius + suspensionData.suspensionLength),
                        Filter = filter
                    };

                    if (physicsWorld.CastRay(raycastInput, out var hit))
                    {
                        raycastData.IsHitThisFrame = true;
                        raycastData.VelocityAtWheel = physicsWorld.GetLinearVelocity(vehicleRbIndex, hit.Position);
                        raycastData.HitPosition = hit.Position;
                        raycastData.HitRBIndex = hit.RigidBodyIndex;
                    }
                    else
                    {
                        raycastData.IsHitThisFrame = false;
                    }
                }).ScheduleParallel(Dependency).Complete();
        }
    }
}
