using Traffic.VehicleComponents.DriveVehicle;
using Traffic.VehicleComponents.Wheel;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Traffic.VehicleSystems
{
    [UpdateInGroup(typeof(VehiclesProcessUpdateSystemGroup))]
    public class UpdateWheelRotationSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _commandBufferSystem;

        protected override void OnCreate()
        {
            _commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        
        protected override void OnUpdate()
        {
            var commandBuffer = _commandBufferSystem.CreateCommandBuffer();
            var parallelCommandBuffer = commandBuffer.AsParallelWriter();
            
            // for drive wheels
            var vehicleEngineComponents = GetComponentDataFromEntity<VehicleEngineData>(true);
            var rotationComponents = GetComponentDataFromEntity<Rotation>(true);
            
            Entities
                .WithReadOnly(vehicleEngineComponents)
                .WithReadOnly(rotationComponents)
                .WithAll<DriveWheelTag>()
                .ForEach((int nativeThreadIndex, in WheelData wheelData, in VehicleRefData vehicleRef) =>
                {
                    var engineData = vehicleEngineComponents[vehicleRef.Entity];
                    var rotationAngle = engineData.maxSpeed * engineData.acceleration / wheelData.radius;
                    
                    rotationAngle = math.radians(rotationAngle);
                    var rotation = rotationComponents[wheelData.wheelModel].Value;
                    rotation = math.mul(rotation, quaternion.RotateZ(-rotationAngle));
                    
                    parallelCommandBuffer.SetComponent(nativeThreadIndex, wheelData.wheelModel,
                        new Rotation {Value = rotation});
                }).ScheduleParallel(Dependency).Complete();
            
            // for all wheels
            Entities
                .WithReadOnly(rotationComponents)
                .ForEach((int nativeThreadIndex, in WheelData wheelData, in WheelRaycastData raycastData, in LocalToWorld wheelRoot) =>
                {
                    var velocityForward = math.dot(raycastData.VelocityAtWheel, wheelRoot.Forward);
                    var rotationAngle = velocityForward / wheelData.radius;
                    
                    rotationAngle = math.radians(rotationAngle);
                    var rotation = rotationComponents[wheelData.wheelModel].Value;
                    rotation = math.mul(rotation, quaternion.RotateZ(-rotationAngle));

                    parallelCommandBuffer.SetComponent(nativeThreadIndex, wheelData.wheelModel,
                        new Rotation {Value = rotation});
                }).ScheduleParallel(Dependency).Complete();
        }
    }
}
