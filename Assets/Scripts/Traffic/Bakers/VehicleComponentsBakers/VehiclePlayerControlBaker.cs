using TrafficSimulation.Traffic.VehicleComponents.DriveVehicle;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Bakers.VehicleComponentsBakers
{
    public class VehiclePlayerControlBaker : Baker<VehiclePlayerControlAuthoring>
    {
        public override void Bake(VehiclePlayerControlAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent<VehiclePlayerControlComponent>(entity);
        }
    }
}