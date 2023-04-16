using TrafficSimulation.Traffic.VehicleComponents.DriveVehicle;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Bakers.VehicleComponentsBakers
{
    public class VehicleSteeringBaker : Baker<VehicleSteeringAuthoring>
    {
        public override void Bake(VehicleSteeringAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new VehicleSteeringData
            {
                MaxAngle = authoring.maxAngle,
                SteeringSpeed = authoring.steeringSpeed
            });
        }
    }
}