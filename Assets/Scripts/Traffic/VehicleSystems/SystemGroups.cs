using Core.Systems;
using Unity.Entities;
using Unity.Physics.Systems;

namespace Traffic.VehicleSystems
{
    // fixed
    
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(BuildPhysicsWorld))]
    public class VehiclesFixedStepSystemGroup : ComponentSystemGroup
    {}
    
    [UpdateInGroup(typeof(VehiclesFixedStepSystemGroup))]
    public class PreprocessVehiclesSystemGroup : ComponentSystemGroup
    {}
    
    [UpdateInGroup(typeof(VehiclesFixedStepSystemGroup))]
    [UpdateAfter(typeof(PreprocessVehiclesSystemGroup))]
    public class ProcessVehiclesSystemGroup : ComponentSystemGroup
    {}
    
    [UpdateInGroup(typeof(VehiclesFixedStepSystemGroup))]
    [UpdateAfter(typeof(ProcessVehiclesSystemGroup))]
    public class AfterProcessVehiclesSystemGroup : ComponentSystemGroup
    {}
    
    // update
    
    [UpdateAfter(typeof(FrameStartSimulationSystemGroup))]
    public class VehiclesUpdateSystemGroup : ComponentSystemGroup
    {}
    
    [UpdateInGroup(typeof(VehiclesUpdateSystemGroup))]
    public class VehiclesProcessUpdateSystemGroup : ComponentSystemGroup
    {}
    
    [UpdateInGroup(typeof(VehiclesUpdateSystemGroup))]
    [UpdateAfter(typeof(VehiclesProcessUpdateSystemGroup))]
    public class VehicleAfterProcessUpdateSystemGroup : ComponentSystemGroup
    {}
    
    // AI
    
    [UpdateAfter(typeof(VehiclesUpdateSystemGroup))]
    public class VehiclesAISystemGroup : ComponentSystemGroup
    {}
    
    [UpdateInGroup(typeof(VehiclesAISystemGroup))]
    public class PreprocessAISystemGroup : ComponentSystemGroup
    {}
    
    [UpdateInGroup(typeof(VehiclesAISystemGroup))]
    [UpdateAfter(typeof(PreprocessAISystemGroup))]
    public class ProcessAISystemGroup : ComponentSystemGroup
    {}
}
