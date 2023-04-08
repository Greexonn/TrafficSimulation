using Unity.Entities;

namespace Traffic.VehicleComponents
{
    public struct VehicleAICollisionDetectionComponent : IComponentData
    {
        public Entity LeftRayPoint, RightRayPoint;
    }
}
