using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace TrafficSimulation.Traffic
{
    [Serializable]
    public struct TrafficSystemData : ICleanupComponentData
    {
        public UnsafeList<NativeParallelMultiHashMap<Entity, Entity>> Graphs;
    }
}