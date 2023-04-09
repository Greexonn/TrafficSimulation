using TrafficSimulation.Traffic.VehicleComponents.DriveVehicle;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Bakers.VehicleComponentsBakers
{
    public class VehicleEngineBaker : Baker<VehicleEngineAuthoring>
    {
        public override void Bake(VehicleEngineAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new VehicleEngineData { MaxSpeed = authoring.maxSpeed });
        }
    }
}