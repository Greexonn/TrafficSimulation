using Unity.Entities;

namespace TrafficSimulation.Traffic.VehicleComponents
{
    public struct VehicleCurrentNodeData : IComponentData
    {
        public Entity Node;
    }
}
