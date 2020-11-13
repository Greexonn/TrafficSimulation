using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Traffic.RoadComponents
{
    [DisallowMultipleComponent]
    public class RoadChunkAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public NativeMultiHashMap<Entity, Entity> chunkGraph;

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
                chunkGraph = new NativeMultiHashMap<Entity, Entity>(linesCount, Allocator.Persistent);
            }
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {       
            TrafficSystem.instance.graphs.Add(chunkGraph);
        }
    }
}
