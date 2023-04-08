using Core.Systems;
using Traffic.RoadSystems;
using Traffic.VehicleComponents;
using Traffic.VehicleComponents.Wheel;
using Unity.Collections;
using Unity.Entities;

namespace Traffic.VehicleSystems
{
    [UpdateInGroup(typeof(FrameStartSimulationSystemGroup))]
    [UpdateAfter(typeof(VehicleSpawnSystem))]
    public partial class VehicleAfterSpawnConfigureSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithStructuralChanges()
                .WithAll<JustSpawnedTag, VehicleTag>()
                .ForEach((Entity vehicleEntity) =>
                {
                    // configure wheels
                    var wheelsBuffer = EntityManager.GetBuffer<WheelElement>(vehicleEntity).Reinterpret<Entity>()
                        .ToNativeArray(Allocator.Temp);

                    var vehicleRefData = new VehicleRefData
                    {
                        Entity = vehicleEntity,
                        WheelsCount = wheelsBuffer.Length
                    };

                    foreach (var wheelEntity in wheelsBuffer)
                    {
                        EntityManager.AddComponentData(wheelEntity, vehicleRefData);
                    }
                    
                    // configure brake wheels
                    var brakeWheelsBuffer = EntityManager.GetBuffer<BrakeWheelElement>(vehicleEntity).Reinterpret<int>()
                        .ToNativeArray(Allocator.Temp);

                    foreach (var wheelId in brakeWheelsBuffer)
                    {
                        EntityManager.AddComponent<BrakeWheelTag>(wheelsBuffer[wheelId]);
                    }
                    
                    // configure drive wheels
                    var driveWheelsBuffer = EntityManager.GetBuffer<DriveWheelElement>(vehicleEntity).Reinterpret<int>()
                        .ToNativeArray(Allocator.Temp);

                    foreach (var wheelId in driveWheelsBuffer)
                    {
                        EntityManager.AddComponent<DriveWheelTag>(wheelsBuffer[wheelId]);
                    }
                    
                    // configure drive wheels
                    var controlWheelsBuffer = EntityManager.GetBuffer<ControlWheelElement>(vehicleEntity).Reinterpret<int>()
                        .ToNativeArray(Allocator.Temp);

                    foreach (var wheelId in controlWheelsBuffer)
                    {
                        EntityManager.AddComponent<ControlWheelTag>(wheelsBuffer[wheelId]);
                    }
                    
                    wheelsBuffer.Dispose();
                    brakeWheelsBuffer.Dispose();
                    driveWheelsBuffer.Dispose();
                    controlWheelsBuffer.Dispose();
                }).Run();
        }
    }
}
