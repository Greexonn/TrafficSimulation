using Core.Systems;
using Traffic.RoadComponents;
using Traffic.VehicleComponents;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Traffic.RoadSystems
{
    [UpdateInGroup(typeof(FrameStartSimulationSystemGroup))]
    public class VehicleSpawnSystem : SystemBase
    {
        private const float SecondsTillSpawn = 10;

        private float _lastSpawnTime;
        
        protected override void OnCreate()
        {
            _lastSpawnTime = -SecondsTillSpawn + 1;
        }

        protected override void OnUpdate()
        {
            var currentTime = (float)Time.ElapsedTime;

            if (currentTime - _lastSpawnTime < SecondsTillSpawn)
                return;

            Entities
                .WithStructuralChanges()
                .WithAll<CarSpawnerComponent>()
                .ForEach((Entity spawnerEntity, ref LocalToWorld transform) => 
            {
                var position = transform.Position + math.up() * 1.0f;

                var index = UnityEngine.Random.Range(0, CarPrefabsStorage.instance.carPrefabs.Length);

                var vehicleEntity = EntityManager.Instantiate(CarPrefabsStorage.instance.carPrefabs[index]);
                EntityManager.SetComponentData(vehicleEntity, new Translation{Value = position});
                EntityManager.AddComponentData(vehicleEntity, new VehicleCurrentNodeData{node = spawnerEntity});
                EntityManager.AddComponentData(vehicleEntity, new VehiclePathNodeIndexData{value = 0});
                EntityManager.AddBuffer<NodeBufferElement>(vehicleEntity);
                //
                EntityManager.AddComponent(vehicleEntity, typeof(PathfindingRequest));
                EntityManager.AddComponent<JustSpawnedTag>(vehicleEntity);
            }).Run();

            _lastSpawnTime = currentTime;
        }
    }
}