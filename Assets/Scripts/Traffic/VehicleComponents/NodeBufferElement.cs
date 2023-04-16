using Unity.Entities;

namespace TrafficSimulation.Traffic.VehicleComponents
{
    public struct NodeBufferElement : IBufferElementData
    {
        public Entity Node;
    }
}
