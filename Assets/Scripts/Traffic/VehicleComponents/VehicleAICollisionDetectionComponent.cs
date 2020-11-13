using Unity.Entities;

namespace Traffic.VehicleComponents
{
    [GenerateAuthoringComponent]
    public struct VehicleAICollisionDetectionComponent : IComponentData
    {
        public Entity leftRayPoint, rightRayPoint;
    }
}
