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
                .WithAll<RoadChunkInitializationData, RoadChunkTag>()
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

            foreach (var (roadChunkInitializationData, entity) in Query<RoadChunkInitializationData>().WithNone<RoadChunkData>().WithEntityAccess())
            {
                ref var roadLinesArray = ref roadChunkInitializationData.LinesBlobArrayRef.Value;
                var roadChunkData = new RoadChunkData
                {
                    ChunkGraph = new NativeParallelMultiHashMap<Entity, Entity>(roadLinesArray.Length, Allocator.Persistent)
                };

                for (var i = 0; i < roadLinesArray.Length; i++)
                {
                    var roadLine = roadLinesArray[i];
                    roadChunkData.ChunkGraph.Add(roadLine.A, roadLine.B);
                }
                
                trafficSystemDataRef.ValueRW.Graphs.Add(roadChunkData.ChunkGraph);
                
                commandBuffer.AddComponent(entity, roadChunkData);
                commandBuffer.RemoveComponent<RoadChunkInitializationData>(entity);
            }
        }
    }
}