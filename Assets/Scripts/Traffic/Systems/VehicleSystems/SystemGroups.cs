using TrafficSimulation.Core.Systems;
using Unity.Entities;
using Unity.Physics.Systems;

namespace TrafficSimulation.Traffic.Systems.VehicleSystems
{
    // fixed
    
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(BuildPhysicsWorld))]
    public partial class VehiclesFixedStepSystemGroup : ComponentSystemGroup
    {}
    
    [UpdateInGroup(typeof(VehiclesFixedStepSystemGroup))]
    public partial class PreprocessVehiclesSystemGroup : ComponentSystemGroup
    {}
    
    [UpdateInGroup(typeof(VehiclesFixedStepSystemGroup))]
    [UpdateAfter(typeof(PreprocessVehiclesSystemGroup))]
    public partial class ProcessVehiclesSystemGroup : ComponentSystemGroup
    {}
    
    [UpdateInGroup(typeof(VehiclesFixedStepSystemGroup))]
    [UpdateAfter(typeof(ProcessVehiclesSystemGroup))]
    public partial class AfterProcessVehiclesSystemGroup : ComponentSystemGroup
    {}
    
    // update
    
    [UpdateAfter(typeof(FrameStartSimulationSystemGroup))]
    public partial class VehiclesUpdateSystemGroup : ComponentSystemGroup
    {}
    
    [UpdateInGroup(typeof(VehiclesUpdateSystemGroup))]
    public partial class VehiclesProcessUpdateSystemGroup : ComponentSystemGroup
    {}
    
    [UpdateInGroup(typeof(VehiclesUpdateSystemGroup))]
    [UpdateAfter(typeof(VehiclesProcessUpdateSystemGroup))]
    public partial class VehicleAfterProcessUpdateSystemGroup : ComponentSystemGroup
    {}
    
    // AI
    
    [UpdateAfter(typeof(VehiclesUpdateSystemGroup))]
    public partial class VehiclesAISystemGroup : ComponentSystemGroup
    {}
    
    [UpdateInGroup(typeof(VehiclesAISystemGroup))]
    public partial class PreprocessAISystemGroup : ComponentSystemGroup
    {}
    
    [UpdateInGroup(typeof(VehiclesAISystemGroup))]
    [UpdateAfter(typeof(PreprocessAISystemGroup))]
    public partial class ProcessAISystemGroup : ComponentSystemGroup
    {}
}
