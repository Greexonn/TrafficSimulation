using TrafficSimulation.Traffic.RoadComponents;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Bakers.RoadComponentsBakers
{
    public class RoadNodeBaker : Baker<RoadNodeAuthoring>
    {
        public override void Bake(RoadNodeAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.WorldSpace | TransformUsageFlags.Dynamic);
            AddComponent(entity, new RoadNodeData { IsOpen = true });
        }
    }
}