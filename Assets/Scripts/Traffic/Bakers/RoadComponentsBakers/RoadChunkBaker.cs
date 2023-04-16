using System.Linq;
using TrafficSimulation.Core.Components;
using TrafficSimulation.Traffic.RoadComponents;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Bakers.RoadComponentsBakers
{
    public class RoadChunkBaker : Baker<RoadChunkAuthoring>
    {
        public override void Bake(RoadChunkAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);

            var roadBlocks = GetComponentsInChildren<RoadBlock>();
            
            foreach (var block in roadBlocks)
            {
                block.ConnectNodes();
            }
            
            var linesCount = roadBlocks.Sum(block => block.GetLinesCount());
            if (linesCount > 0)
            {
                var initializationBuffer = AddBuffer<RoadChunkLineInitializationBufferElement>(entity);
                foreach (var roadBlock in roadBlocks)
                {
                    roadBlock.Bake(this, initializationBuffer);
                }
            }
            
            AddComponent<RoadChunkTag>(entity);
            AddComponent<JustInstantiatedTag>(entity);
        }
    }
}