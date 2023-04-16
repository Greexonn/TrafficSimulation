using System;
using Unity.Entities;

namespace TrafficSimulation.Traffic.RoadComponents.TrafficControl
{
    [Serializable]
    public struct TrafficControlBlockComponent : IComponentData
    {
        public int groupsCount;
        public int statesCount;
    }
}
