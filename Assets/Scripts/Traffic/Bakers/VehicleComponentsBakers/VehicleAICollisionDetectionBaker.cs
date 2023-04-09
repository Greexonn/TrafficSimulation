using TrafficSimulation.Traffic.VehicleComponents;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Bakers.VehicleComponentsBakers
{
    public class VehicleAICollisionDetectionBaker : Baker<VehicleAICollisionDetectionAuthoring>
    {
        public override void Bake(VehicleAICollisionDetectionAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new VehicleAICollisionDetectionComponent
            {
                LeftRayPoint = GetEntity(authoring.leftRayPoint, TransformUsageFlags.Dynamic),
                RightRayPoint = GetEntity(authoring.rightRayPoint, TransformUsageFlags.Dynamic)
            });
        }
    }
}