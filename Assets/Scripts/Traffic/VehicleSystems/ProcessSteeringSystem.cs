using Traffic.VehicleComponents.DriveVehicle;
using Traffic.VehicleComponents.Wheel;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Traffic.VehicleSystems
{
    [UpdateInGroup(typeof(VehiclesProcessUpdateSystemGroup))]
    public class ProcessSteeringSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var localToWorldComponents = GetComponentDataFromEntity<LocalToWorld>(true);
            var vehicleSteeringComponents = GetComponentDataFromEntity<VehicleSteeringData>(true);
            
            Entities
                .WithReadOnly(localToWorldComponents)
                .WithReadOnly(vehicleSteeringComponents)
                .WithAll<ControlWheelTag>()
                .ForEach((ref Rotation wheelRotation, in VehicleRefData vehicleRef) =>
                {
                    var vehicleTransforms = localToWorldComponents[vehicleRef.Entity];
                    var steeringData = vehicleSteeringComponents[vehicleRef.Entity];
                    
                    wheelRotation.Value = math.mul(math.inverse(vehicleTransforms.Rotation),
                        steeringData.currentRotation);
                }).ScheduleParallel();
        }
    }
}
