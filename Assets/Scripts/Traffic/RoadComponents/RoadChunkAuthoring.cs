using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Traffic.RoadComponents
{
    [DisallowMultipleComponent]
    public class RoadChunkAuthoring : MonoBehaviour
    {
        public NativeParallelMultiHashMap<Entity, Entity> ChunkGraph;

        private void Awake()
        {
            var roadBlocks = GetComponentsInChildren<RoadBlockAuthoring>();

            foreach (var block in roadBlocks)
            {
                block.ConnectNodes();
            }

            var linesCount = 0;

            foreach (var block in roadBlocks)
            {
                block._parentChunk = this;
                linesCount += block.GetLinesCount();
            }

            if (linesCount > 0)
            {
                ChunkGraph = new NativeParallelMultiHashMap<Entity, Entity>(linesCount, Allocator.Persistent);
            }
        }

        public void Convert(Entity entity, EntityManager dstManager)
        {       
            TrafficSystem.Instance.Graphs.Add(ChunkGraph);
        }
    }
}
