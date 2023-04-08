using Traffic.VehicleComponents.DriveVehicle;
using Traffic.VehicleComponents.Wheel;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Traffic.VehicleSystems
{
    [UpdateInGroup(typeof(VehiclesProcessUpdateSystemGroup))]
    public partial class ProcessSteeringSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<ControlWheelTag>()
                .ForEach((ref LocalTransform wheelTransform, in VehicleRefData vehicleRef) =>
                {
                    var vehicleTransforms = SystemAPI.GetComponent<LocalToWorld>(vehicleRef.Entity);
                    var steeringData = SystemAPI.GetComponent<VehicleSteeringData>(vehicleRef.Entity);
                    
                    wheelTransform.Rotation = math.mul(math.inverse(vehicleTransforms.Rotation), steeringData.CurrentRotation);
                }).ScheduleParallel();
        }
    }
}
