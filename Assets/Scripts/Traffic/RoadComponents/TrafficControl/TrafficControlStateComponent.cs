using System;
using Unity.Entities;

namespace TrafficSimulation.Traffic.RoadComponents.TrafficControl
{
    [Serializable]
    public struct TrafficControlStateComponent : IComponentData
    {
        public int stateId;
        public float stateRemainingTime;
    }
}
