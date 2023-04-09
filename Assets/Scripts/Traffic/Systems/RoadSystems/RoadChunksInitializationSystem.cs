using TrafficSimulation.Core.Components;
using TrafficSimulation.Traffic.RoadComponents;
using Unity.Burst;
using Unity.Entities;
using static Unity.Entities.SystemAPI;

namespace TrafficSimulation.Traffic.Systems.RoadSystems
{
    public partial struct RoadChunksInitializationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TrafficSystemData>();
            state.RequireForUpdate(new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<TrafficSystemData, TrafficSystemValidTag>().Build(ref state));
            state.RequireForUpdate(new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<RoadChunkData, RoadChunkValidTag, JustInstantiatedTag>().Build(ref state));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var trafficSystemData = GetSingleton<TrafficSystemData>();

            foreach (var roadChunkData in Query<RoadChunkData>().WithAll<JustInstantiatedTag>())
            {
                trafficSystemData.Graphs.Add(roadChunkData.ChunkGraph);
            }
        }
    }
}