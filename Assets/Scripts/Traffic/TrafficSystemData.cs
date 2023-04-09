using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace TrafficSimulation.Traffic
{
    [Serializable]
    public struct TrafficSystemData : IComponentData
    {
        public UnsafeList<NativeParallelMultiHashMap<Entity, Entity>> Graphs;
    }
}