using TrafficSimulation.Core.Systems;
using TrafficSimulation.Traffic.Behaviours;
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
        }

        protected override void OnUpdate()
        {
            var currentTime = (float)SystemAPI.Time.ElapsedTime;

            if (currentTime - _lastSpawnTime < SecondsTillSpawn)
                return;

            Entities
                .WithStructuralChanges()
                .WithAll<CarSpawnerComponent>()
                .ForEach((Entity spawnerEntity, ref LocalToWorld transform) => 
            {
                var position = transform.Position + math.up() * 1.0f;

                var index = UnityEngine.Random.Range(0, CarPrefabsStorage.Instance.CarPrefabs.Length);

                var vehicleEntity = EntityManager.Instantiate(CarPrefabsStorage.Instance.CarPrefabs[index]);
                EntityManager.SetComponentData(vehicleEntity, new LocalTransform { Position = position, Rotation = quaternion.identity, Scale = 1f });
                EntityManager.AddComponentData(vehicleEntity, new VehicleCurrentNodeData{Node = spawnerEntity});
                EntityManager.AddComponentData(vehicleEntity, new VehiclePathNodeIndexData{Value = 0});
                EntityManager.AddBuffer<NodeBufferElement>(vehicleEntity);
                //
                EntityManager.AddComponent(vehicleEntity, typeof(PathfindingRequest));
                EntityManager.AddComponent<JustSpawnedTag>(vehicleEntity);
            }).Run();

            _lastSpawnTime = currentTime;
        }
    }
}