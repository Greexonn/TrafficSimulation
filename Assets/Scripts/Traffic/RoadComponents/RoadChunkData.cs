using System;
using Unity.Collections;
using Unity.Entities;

namespace TrafficSimulation.Traffic.RoadComponents
{
    [Serializable]
    public struct RoadChunkData : IComponentData
    {
        public int LinesCount;
        public NativeParallelMultiHashMap<Entity, Entity> ChunkGraph;
    }
}