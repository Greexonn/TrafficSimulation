using TrafficSimulation.Core.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using static Unity.Entities.SystemAPI;

namespace TrafficSimulation.Traffic.Systems
{
    public partial struct TrafficSystemInitializationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate(new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<TrafficSystemTag, JustInstantiatedTag>()
                .WithNone<TrafficSystemData>()
                .Build(ref state));
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (_, entity) in Query<TrafficSystemTag>().WithAll<JustInstantiatedTag>().WithNone<TrafficSystemData>().WithEntityAccess())
            {
                commandBuffer.AddComponent(entity, new TrafficSystemData
                {
                    Graphs = new UnsafeList<NativeParallelMultiHashMap<Entity, Entity>>(10, Allocator.Persistent)
                });
            }
        }
    }
}