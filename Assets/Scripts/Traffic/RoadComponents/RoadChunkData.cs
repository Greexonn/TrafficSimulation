using System;
using Unity.Collections;
using Unity.Entities;

namespace TrafficSimulation.Traffic.RoadComponents
{
    [Serializable]
    public struct RoadChunkData : ICleanupComponentData
    {
        public NativeParallelMultiHashMap<Entity, Entity> ChunkGraph;
    }

    [Serializable]
    public struct RoadChunkInitializationData : IComponentData
    {
        public BlobAssetReference<BlobArray<RoadLineBlobData>> LinesBlobArrayRef;
    }

    [Serializable]
    public struct RoadLineBlobData
    {
        public Entity A, B;
    }
}