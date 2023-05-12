using TrafficSimulation.Traffic.VehicleComponents.DriveVehicle;
using TrafficSimulation.Traffic.VehicleComponents.Wheel;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace TrafficSimulation.Traffic.Systems.VehicleSystems
{
    [UpdateInGroup(typeof(VehiclesProcessUpdateSystemGroup))]
    public partial struct UpdateWheelsRotationsSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate(new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<WheelData, LocalToWorld>()
                .Build(ref state));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var updateDriveWheelsRotationsJob = new UpdateDriveWheelsRotationsJob
            {
                VehicleEngineDataLookup = GetComponentLookup<VehicleEngineData>(),
                LocalTransformLookup = GetComponentLookup<LocalTransform>()
            };
            var handle = updateDriveWheelsRotationsJob.ScheduleParallel(state.Dependency);

            var updateAllWheelsRotationsJob = new UpdateAllWheelsRotationsJob
            {
                LocalTransformLookup = GetComponentLookup<LocalTransform>()
            };
            handle = updateAllWheelsRotationsJob.ScheduleParallel(handle);
            state.Dependency = handle;
        }
        
        [BurstCompile]
        private partial struct UpdateDriveWheelsRotationsJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<VehicleEngineData> VehicleEngineDataLookup;
            [NativeDisableContainerSafetyRestriction] public ComponentLookup<LocalTransform> LocalTransformLookup;

            private void Execute(in WheelData wheelData, in VehicleRefData vehicleRef)
            {
                var engineData = VehicleEngineDataLookup[vehicleRef.Entity];
                var rotationAngle = engineData.MaxSpeed * engineData.Acceleration / wheelData.radius;
                    
                rotationAngle = math.radians(rotationAngle);
                var wheelModelTransformRef = LocalTransformLookup.GetRefRW(wheelData.wheelModel);
                wheelModelTransformRef.ValueRW.Rotation = math.mul(wheelModelTransformRef.ValueRO.Rotation, quaternion.RotateZ(-rotationAngle));
            }
        }
        
        [BurstCompile]
        private partial struct UpdateAllWheelsRotationsJob : IJobEntity
        {
            [NativeDisableContainerSafetyRestriction] public ComponentLookup<LocalTransform> LocalTransformLookup;
            
            private void Execute(in WheelData wheelData, in WheelRaycastData raycastData, in LocalToWorld wheelRoot)
            {
                var velocityForward = math.dot(raycastData.VelocityAtWheel, wheelRoot.Forward);
                var rotationAngle = velocityForward / (wheelData.radius * 2f);
                    
                rotationAngle = math.radians(rotationAngle);
                var wheelModelTransformRef = LocalTransformLookup.GetRefRW(wheelData.wheelModel);
                wheelModelTransformRef.ValueRW.Rotation = math.mul(wheelModelTransformRef.ValueRO.Rotation, quaternion.RotateZ(-rotationAngle));
            }
        }
    }
}
