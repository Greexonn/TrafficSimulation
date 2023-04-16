using TrafficSimulation.Traffic.VehicleComponents.DriveVehicle;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Bakers.VehicleComponentsBakers
{
    public class VehicleBrakesBaker : Baker<VehicleBrakesAuthoring>
    {
        public override void Bake(VehicleBrakesAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent<VehicleBrakesData>(entity);
        }
    }
}