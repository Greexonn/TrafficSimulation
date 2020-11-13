using Unity.Entities;

namespace Traffic.VehicleComponents
{
    public struct VehicleCurrentNodeData : IComponentData
    {
        public Entity node;
    }
}
