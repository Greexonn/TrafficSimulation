using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class CarSpawnSystem : ComponentSystem
{
    private float _secondsTillSpawn = 3;

    private float _lastSpawnTime = 0;

    private EntityManager _manager;


    protected override void OnCreate()
    {
        _manager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    protected override void OnUpdate()
    {
        float _currentTime = (float)Time.ElapsedTime;

        if ((_currentTime - _lastSpawnTime) < _secondsTillSpawn)
            return;

        Entities.ForEach((Entity spawnerEntity, ref CarSpawnerComponent spawner, ref LocalToWorld transform) => 
        {
            int _index = UnityEngine.Random.Range(0, CarPrefabsStorage.instance.carPrefabs.Length);

            Entity _vehicleEntity = _manager.Instantiate(CarPrefabsStorage.instance.carPrefabs[_index]);

            _manager.SetName(_vehicleEntity, "vehicle");
            _manager.SetComponentData(_vehicleEntity, new Translation{Value = transform.Position});
            _manager.AddComponentData(_vehicleEntity, new VehicleCurrentNodeComponent{node = spawnerEntity});
            _manager.AddComponentData(_vehicleEntity, new VehiclePathNodeIndexComponent{value = 0});
            _manager.AddBuffer<NodeBufferElement>(_vehicleEntity);
            //
            _manager.AddComponent(_vehicleEntity, typeof(PathfindingRequestComponent));
        });

        _lastSpawnTime = _currentTime;
    }
}