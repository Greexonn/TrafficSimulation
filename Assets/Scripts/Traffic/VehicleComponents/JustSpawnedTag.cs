using System;
using TrafficSimulation.Core.Components;

namespace TrafficSimulation.Traffic.VehicleComponents
{
    [Serializable]
    public struct JustSpawnedTag : IRemoveAtFrameEndComponent
    {}
}
