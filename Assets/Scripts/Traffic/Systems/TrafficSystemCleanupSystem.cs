using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace TrafficSimulation.Traffic.Systems
{
    public partial struct TrafficSystemCleanupSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate(new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<TrafficSystemData>()
                .WithNone<TrafficSystemTag>()
                .Build(ref state));
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (trafficSystemDataRef, entity) in Query<RefRW<TrafficSystemData>>().WithNone<TrafficSystemTag>().WithEntityAccess())
            {
                commandBuffer.RemoveComponent<TrafficSystemData>(entity);
                
                Debug.Log("####cleanup");
                if (!trafficSystemDataRef.ValueRO.Graphs.IsCreated)
                    continue;

                trafficSystemDataRef.ValueRW.Graphs.Dispose(state.Dependency);
            }
        }
    }
}