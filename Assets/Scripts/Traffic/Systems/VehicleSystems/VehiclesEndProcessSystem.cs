using TrafficSimulation.Core.Systems;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Systems.VehicleSystems
{
    [UpdateInGroup(typeof(AfterProcessVehiclesSystemGroup))]
    public partial class VehiclesEndProcessSystem : SystemWithPublicDependencyBase
    {
        private SystemWithPublicDependencyBase _lastSystem;

        protected override void OnCreate()
        {
            _lastSystem = World.GetOrCreateSystemManaged<ProcessDriveWheelsSystem>();
        }

        protected override void OnUpdate()
        {
            Dependency = _lastSystem.PublicDependency;
            Dependency.Complete();
        }
    }
}
