using TrafficSimulation.Traffic.VehicleComponents;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Bakers.VehicleComponentsBakers
{
    public class VehicleAIBaker : Baker<VehicleAIAuthoring>
    {
        public override void Bake(VehicleAIAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new VehicleAIData
            {
                VehicleAITransform = GetEntity(authoring.vehicleAITransform, TransformUsageFlags.Dynamic)
            });
        }
    }
}