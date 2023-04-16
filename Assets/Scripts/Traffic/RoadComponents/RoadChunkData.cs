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

    [Serializable, InternalBufferCapacity(64)]
    public struct RoadChunkLineInitializationBufferElement : IBufferElementData
    {
        public Entity A, B;
    }
}