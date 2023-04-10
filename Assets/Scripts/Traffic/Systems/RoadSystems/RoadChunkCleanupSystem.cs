using TrafficSimulation.Traffic.RoadComponents;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace TrafficSimulation.Traffic.Systems.RoadSystems
{
    [UpdateBefore(typeof(TrafficSystemCleanupSystem))]
    public partial struct RoadChunkCleanupSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate(new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<RoadChunkData>()
                .WithNone<RoadChunkTag>()
                .Build(ref state));
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (roadChunkDataRef, entity) in Query<RefRW<RoadChunkData>>().WithNone<RoadChunkTag>().WithEntityAccess())
            {
                commandBuffer.RemoveComponent<RoadChunkData>(entity);
                
                Debug.Log("####cleanup road chunk");
                if (!roadChunkDataRef.ValueRO.ChunkGraph.IsCreated)
                    continue;

                roadChunkDataRef.ValueRW.ChunkGraph.Dispose(state.Dependency);
            }
        }
    }
}