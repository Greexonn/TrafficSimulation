using Core.Systems;
using Traffic.VehicleComponents;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Traffic.RoadSystems
{
    [UpdateInGroup(typeof(FrameStartSimulationSystemGroup))]
    public class VehicleSpawnSystem : ComponentSystem
    {
        private const float SecondsTillSpawn = 20;

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

            Entities.ForEach((Entity spawnerEntity, ref CarSpawnerComponent spawner, ref LocalToWorld transform) => 
            {
                var position = transform.Position + math.up() * 1.0f;

                var index = UnityEngine.Random.Range(0, CarPrefabsStorage.instance.carPrefabs.Length);

                var vehicleEntity = EntityManager.Instantiate(CarPrefabsStorage.instance.carPrefabs[index]);
#if UNITY_EDITOR
                EntityManager.SetName(vehicleEntity, "vehicle");
#endif
                EntityManager.SetComponentData(vehicleEntity, new Translation{Value = position});
                EntityManager.AddComponentData(vehicleEntity, new VehicleCurrentNodeComponent{node = spawnerEntity});
                EntityManager.AddComponentData(vehicleEntity, new VehiclePathNodeIndexComponent{value = 0});
                EntityManager.AddBuffer<NodeBufferElement>(vehicleEntity);
                //
                EntityManager.AddComponent(vehicleEntity, typeof(PathfindingRequestComponent));
                EntityManager.AddComponent<JustSpawnedTag>(vehicleEntity);
            });

            _lastSpawnTime = currentTime;
        }
    }
}