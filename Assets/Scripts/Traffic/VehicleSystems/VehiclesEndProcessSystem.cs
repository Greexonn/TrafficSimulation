using Core.Systems;
using Unity.Entities;

namespace Traffic.VehicleSystems
{
    [UpdateInGroup(typeof(AfterProcessVehiclesSystemGroup))]
    public class VehiclesEndProcessSystem : SystemWithPublicDependencyBase
    {
        private SystemWithPublicDependencyBase _lastSystem;

        protected override void OnCreate()
        {
            _lastSystem = World.GetOrCreateSystem<ProcessDriveWheelsSystem>();
        }

        protected override void OnUpdate()
        {
            Dependency = _lastSystem.PublicDependency;
            Dependency.Complete();
        }
    }
}
