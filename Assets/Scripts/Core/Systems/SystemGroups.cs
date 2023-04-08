using Unity.Entities;

namespace TrafficSimulation.Core.Systems
{
    public partial class FrameStartSimulationSystemGroup : ComponentSystemGroup
    {}
    
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class FinishingSystemGroup : ComponentSystemGroup
    {}
}
