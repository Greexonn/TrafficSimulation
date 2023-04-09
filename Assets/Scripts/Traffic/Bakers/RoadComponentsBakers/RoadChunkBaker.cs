using System.Linq;
using TrafficSimulation.Core.Components;
using TrafficSimulation.Traffic.RoadComponents;
using Unity.Collections;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Bakers.RoadComponentsBakers
{
    public class RoadChunkBaker : Baker<RoadChunkAuthoring>
    {
        public override void Bake(RoadChunkAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            var component = new RoadChunkData();
            
            var roadBlocks = GetComponentsInChildren<RoadBlock>();
            
            foreach (var block in roadBlocks)
            {
                block.ConnectNodes();
            }
            
            var linesCount = roadBlocks.Sum(block => block.GetLinesCount());
            component.LinesCount = linesCount;
            if (linesCount > 0)
            {
                component.ChunkGraph = new NativeParallelMultiHashMap<Entity, Entity>(linesCount, Allocator.Persistent);
                foreach (var roadBlock in roadBlocks)
                {
                    roadBlock.Bake(this, component.ChunkGraph);
                }
            }
            
            AddComponent(entity, component);
            AddComponent<RoadChunkValidTag>(entity);
            AddComponent<JustInstantiatedTag>(entity);
        }
    }
}