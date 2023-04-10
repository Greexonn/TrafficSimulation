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
            var initializationData = new RoadChunkInitializationData();
            
            var roadBlocks = GetComponentsInChildren<RoadBlock>();
            
            foreach (var block in roadBlocks)
            {
                block.ConnectNodes();
            }
            
            var linesCount = roadBlocks.Sum(block => block.GetLinesCount());
            if (linesCount > 0)
            {
                var index = 0;
                var blobBuilder = new BlobBuilder(Allocator.Temp);
                ref var blobArrayRoot = ref blobBuilder.ConstructRoot<BlobArray<RoadLineBlobData>>();
                var blobBuilderArray = blobBuilder.Allocate(ref blobArrayRoot, linesCount);

                foreach (var roadBlock in roadBlocks)
                {
                    roadBlock.Bake(this, blobBuilderArray, ref index);
                }

                initializationData.LinesBlobArrayRef = blobBuilder.CreateBlobAssetReference<BlobArray<RoadLineBlobData>>(Allocator.Persistent);
                AddBlobAsset(ref initializationData.LinesBlobArrayRef, out _);
                
                blobBuilder.Dispose();
            }
            
            AddComponent(entity, initializationData);
            AddComponent<RoadChunkTag>(entity);
            AddComponent<JustInstantiatedTag>(entity);
        }
    }
}