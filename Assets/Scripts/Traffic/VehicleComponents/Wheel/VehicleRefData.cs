using System;
using Unity.Entities;

namespace TrafficSimulation.Traffic.VehicleComponents.Wheel
{
    [Serializable]
    public struct VehicleRefData : IComponentData
    {
        public Entity Entity;

        public int WheelsCount;
    }
}
