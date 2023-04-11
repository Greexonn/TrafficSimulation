using TrafficSimulation.Traffic.VehicleComponents.Wheel;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Bakers.VehicleComponentsBakers
{
    public class SuspensionBaker : Baker<SuspensionAuthoring>
    {
        public override void Bake(SuspensionAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);

            var vehicleMass = authoring.body.Mass;
            var suspensionData = authoring.suspension;
            suspensionData.springStrength = vehicleMass / 10 * authoring.springStrengthCoeff;
            suspensionData.damperStrength = vehicleMass / 10 * authoring.damperStrengthCoeff;
            
            AddComponent(entity, suspensionData);
        }
    }
}