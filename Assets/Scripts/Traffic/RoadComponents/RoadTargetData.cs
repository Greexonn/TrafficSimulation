using Unity.Entities;

namespace TrafficSimulation.Traffic.RoadComponents
{
    public struct RoadTargetData : IComponentData
    {
        public Entity Node;
    }
}