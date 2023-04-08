using Traffic.VehicleComponents.DriveVehicle;
using Traffic.VehicleComponents.Wheel;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace Traffic.VehicleSystems
{
    [UpdateInGroup(typeof(VehiclesProcessUpdateSystemGroup))]
    public partial struct UpdateWheelRotationSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var updateDriveWheelsRotationsJob = new UpdateDriveWheelsRotationsJob
            {
                VehicleEngineDataLookup = GetComponentLookup<VehicleEngineData>(true),
                LocalTransformLookup = GetComponentLookup<LocalTransform>()
            };
            var handle = updateDriveWheelsRotationsJob.ScheduleParallelByRef(state.Dependency);

            var updateAllWheelsRotationsJob = new UpdateAllWheelsRotationsJob
            {
                LocalTransformLookup = GetComponentLookup<LocalTransform>()
            };
            handle = updateAllWheelsRotationsJob.ScheduleParallelByRef(handle);
            handle.Complete();
        }
        
        [BurstCompile]
        private partial struct UpdateDriveWheelsRotationsJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<VehicleEngineData> VehicleEngineDataLookup;
            public ComponentLookup<LocalTransform> LocalTransformLookup;

            private void Execute(in WheelData wheelData, in VehicleRefData vehicleRef)
            {
                var engineData = VehicleEngineDataLookup[vehicleRef.Entity];
                var rotationAngle = engineData.MaxSpeed * engineData.Acceleration / wheelData.radius;
                    
                rotationAngle = math.radians(rotationAngle);
                var wheelModelTransformRef = LocalTransformLookup.GetRefRW(wheelData.wheelModel, false);
                wheelModelTransformRef.ValueRW.Rotation = math.mul(wheelModelTransformRef.ValueRO.Rotation, quaternion.RotateZ(-rotationAngle));
            }
        }
        
        [BurstCompile]
        private partial struct UpdateAllWheelsRotationsJob : IJobEntity
        {
            public ComponentLookup<LocalTransform> LocalTransformLookup;
            
            private void Execute(in WheelData wheelData, in WheelRaycastData raycastData, in LocalToWorld wheelRoot)
            {
                var velocityForward = math.dot(raycastData.VelocityAtWheel, wheelRoot.Forward);
                var rotationAngle = velocityForward / wheelData.radius;
                    
                rotationAngle = math.radians(rotationAngle);
                var wheelModelTransformRef = LocalTransformLookup.GetRefRW(wheelData.wheelModel, false);
                wheelModelTransformRef.ValueRW.Rotation = math.mul(wheelModelTransformRef.ValueRO.Rotation, quaternion.RotateZ(-rotationAngle));
            }
        }
    }
}
