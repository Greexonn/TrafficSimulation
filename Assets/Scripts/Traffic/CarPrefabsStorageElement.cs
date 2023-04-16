using Unity.Entities;

namespace TrafficSimulation.Traffic
{
    [InternalBufferCapacity(10)]
    public struct CarPrefabsStorageElement : IBufferElementData
    {
        public Entity PrefabEntity;
    }
}