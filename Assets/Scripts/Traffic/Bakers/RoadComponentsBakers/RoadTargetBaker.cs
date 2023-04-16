using TrafficSimulation.Traffic.RoadComponents;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Bakers.RoadComponentsBakers
{
    public class RoadTargetBaker : Baker<RoadTargetAuthoring>
    {
        public override void Bake(RoadTargetAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new RoadTargetData
            {
                Node = GetEntity(authoring.Node, TransformUsageFlags.Dynamic)
            });
        }
    }
}