using Unity.Entities;

namespace TrafficSimulation.Traffic.RoadComponents.TrafficControl
{
    [InternalBufferCapacity(10)]
    public struct StateTimeBufferElement : IBufferElementData
    {
        public int Value;
    }
}
