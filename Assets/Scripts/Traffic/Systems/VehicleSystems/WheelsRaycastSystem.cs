using TrafficSimulation.Traffic.VehicleComponents.Wheel;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace TrafficSimulation.Traffic.Systems.VehicleSystems
{
    [UpdateInGroup(typeof(PreprocessVehiclesSystemGroup))]
    public partial struct WheelsRaycastSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate(new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<WheelRaycastData, WheelData, SuspensionData, LocalToWorld, VehicleRefData>()
                .Build(ref state));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var physicsSingleton = GetSingleton<PhysicsWorldSingleton>();
            
            var job = new WheelsRaycastJob
            {
                LocalToWorldLookup = GetComponentLookup<LocalToWorld>(true),
                PhysicsSingleton = physicsSingleton
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct WheelsRaycastJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;
            [ReadOnly] public PhysicsWorldSingleton PhysicsSingleton;
            
            private void Execute(ref WheelRaycastData raycastData, in WheelData wheelData, in SuspensionData suspensionData, in LocalToWorld wheelRoot, in VehicleRefData vehicleRef)
            {
                var vehicleRbIndex = PhysicsSingleton.GetRigidBodyIndex(vehicleRef.Entity);
                if (vehicleRbIndex == -1 || vehicleRbIndex >= PhysicsSingleton.NumDynamicBodies)
                    return;
                    
                var suspensionTop = wheelRoot.Position;

                var vehicleTransforms = LocalToWorldLookup[vehicleRef.Entity];
                var dirUp = vehicleTransforms.Up;

                var filter = PhysicsSingleton.PhysicsWorld.GetCollisionFilter(vehicleRbIndex);
                var raycastInput = new RaycastInput
                {
                    Start = suspensionTop,
                    End = suspensionTop - dirUp * (wheelData.radius + suspensionData.suspensionLength),
                    Filter = filter
                };

                if (PhysicsSingleton.CastRay(raycastInput, out var hit))
                {
                    raycastData.IsHitThisFrame = true;
                    raycastData.VelocityAtWheel = PhysicsSingleton.PhysicsWorld.GetLinearVelocity(vehicleRbIndex, hit.Position);
                    raycastData.HitPosition = hit.Position;
                    raycastData.HitRBIndex = hit.RigidBodyIndex;
                }
                else
                {
                    raycastData.IsHitThisFrame = false;
                }
            }
        }
    }
}
