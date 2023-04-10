using TrafficSimulation.Traffic.RoadComponents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using static Unity.Entities.SystemAPI;

namespace TrafficSimulation.Traffic.Systems.RoadSystems
{
    public partial struct RoadChunkInitializationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate(new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<RoadChunkLineInitializationBufferElement, RoadChunkTag>()
                .WithNone<RoadChunkData>()
                .Build(ref state));
            state.RequireForUpdate<TrafficSystemData>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var trafficSystemDataRef = GetSingletonRW<TrafficSystemData>();
            var commandBuffer = GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (_, entity) in Query<RoadChunkTag>().WithNone<RoadChunkData>().WithEntityAccess())
            {
                var roadLinesArray = GetBuffer<RoadChunkLineInitializationBufferElement>(entity).AsNativeArray();
                var roadChunkData = new RoadChunkData
                {
                    ChunkGraph = new NativeParallelMultiHashMap<Entity, Entity>(roadLinesArray.Length, Allocator.Persistent)
                };

                foreach (var roadLine in roadLinesArray)
                {
                    roadChunkData.ChunkGraph.Add(roadLine.A, roadLine.B);
                }
                
                trafficSystemDataRef.ValueRW.Graphs.Add(roadChunkData.ChunkGraph);
                
                commandBuffer.AddComponent(entity, roadChunkData);
                commandBuffer.RemoveComponent<RoadChunkLineInitializationBufferElement>(entity);
            }
        }
    }
}