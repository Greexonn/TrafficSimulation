using Unity.Entities;

namespace TrafficSimulation.Traffic.VehicleComponents
{
    public struct VehicleAICollisionDetectionComponent : IComponentData
    {
        public Entity LeftRayPoint, RightRayPoint;
    }
}
