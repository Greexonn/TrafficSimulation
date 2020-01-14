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

        Entities.ForEach((ref CarSpawnerComponent spawner, ref LocalToWorld transform) => 
        {
            int _index = UnityEngine.Random.Range(0, CarPrefabsStorage.instance.carPrefabs.Length);

            Entity _carEntity = _manager.Instantiate(CarPrefabsStorage.instance.carPrefabs[_index]);

            _manager.SetComponentData(_carEntity, new Translation{Value = transform.Position});
        });

        _lastSpawnTime = _currentTime;
    }
}