using Unity.Entities;

namespace Core.Systems
{
    public partial class FrameStartSimulationSystemGroup : ComponentSystemGroup
    {}
    
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class FinishingSystemGroup : ComponentSystemGroup
    {}
}
