using Unity.Entities;

namespace Core.Systems
{
    public class FrameStartSimulationSystemGroup : ComponentSystemGroup
    {}
    
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class FinishingSystemGroup : ComponentSystemGroup
    {}
}
