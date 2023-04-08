using Core.Systems;
using Unity.Entities;

namespace Traffic.VehicleSystems
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
