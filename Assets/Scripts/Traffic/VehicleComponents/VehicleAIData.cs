using Unity.Entities;

namespace Traffic.VehicleComponents
{
    [GenerateAuthoringComponent]
    public struct VehicleAIData : IComponentData
    {
        public Entity vehicleAITransform;
    }
}
