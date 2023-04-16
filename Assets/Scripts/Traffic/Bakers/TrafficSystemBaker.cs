using TrafficSimulation.Core.Components;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Bakers
{
    public class TrafficSystemBaker : Baker<TrafficSystemAuthoring>
    {
        public override void Bake(TrafficSystemAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent<TrafficSystemTag>(entity);
            AddComponent<JustInstantiatedTag>(entity);
        }
    }
}