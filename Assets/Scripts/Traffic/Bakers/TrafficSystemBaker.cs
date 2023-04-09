using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Bakers
{
    public class TrafficSystemBaker : Baker<TrafficSystemAuthoring>
    {
        public override void Bake(TrafficSystemAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new TrafficSystemData
            {
                Graphs = new UnsafeList<NativeParallelMultiHashMap<Entity, Entity>>(10, Allocator.Persistent)
            });
            AddComponent<TrafficSystemValidTag>(entity);
        }
    }
}