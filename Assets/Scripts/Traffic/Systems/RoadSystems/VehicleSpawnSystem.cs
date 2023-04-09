using TrafficSimulation.Core.Systems;
using TrafficSimulation.Traffic.RoadComponents;
using TrafficSimulation.Traffic.VehicleComponents;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace TrafficSimulation.Traffic.Systems.RoadSystems
{
    [UpdateInGroup(typeof(FrameStartSimulationSystemGroup))]
    public partial class VehicleSpawnSystem : SystemBase
    {
        private const float SecondsTillSpawn = 10;

        private float _lastSpawnTime;
        
        protected override void OnCreate()
        {
            _lastSpawnTime = -SecondsTillSpawn + 1;
            RequireForUpdate<CarPrefabsStorageElement>();
            RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        protected override void OnUpdate()
        {
            var currentTime = (float)SystemAPI.Time.ElapsedTime;

            if (currentTime - _lastSpawnTime < SecondsTillSpawn)
                return;

            var carPrefabsBuffer = SystemAPI.GetSingletonBuffer<CarPrefabsStorageElement>(true);
            var commandBuffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            var random = new Random((uint)UnityEngine.Random.Range(0, int.MaxValue));

            Entities
                .WithAll<CarSpawnerComponent>()
                .ForEach((Entity spawnerEntity, ref LocalToWorld transform) =>
                {
                    var position = transform.Position + math.up() * 1.0f;

                    var index = random.NextInt(0, carPrefabsBuffer.Length);

                    var vehicleEntity = commandBuffer.Instantiate(carPrefabsBuffer[index].PrefabEntity);
                    commandBuffer.SetComponent(vehicleEntity, new LocalTransform { Position = position, Rotation = quaternion.identity, Scale = 1f });
                    commandBuffer.AddComponent(vehicleEntity, new VehicleCurrentNodeData { Node = spawnerEntity });
                    commandBuffer.AddComponent(vehicleEntity, new VehiclePathNodeIndexData { Value = 0 });
                    commandBuffer.AddBuffer<NodeBufferElement>(vehicleEntity);
                    //
                    commandBuffer.AddComponent(vehicleEntity, typeof(PathfindingRequest));
                    commandBuffer.AddComponent<JustSpawnedTag>(vehicleEntity);
                }).Run();

            _lastSpawnTime = currentTime;
        }
    }
}