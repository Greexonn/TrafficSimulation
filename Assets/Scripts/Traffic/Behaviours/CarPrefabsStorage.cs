using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;

public class CarPrefabsStorage : MonoBehaviour
{
    public static CarPrefabsStorage instance;

    [SerializeField] private List<GameObject> _carPrefabs;

    public NativeArray<Entity> carPrefabs;

    void Awake()
    {
        if (instance != this)
        {
            if (instance != null)
                Destroy(this);
            else
                instance = this;
        }
    }

    void Start()
    {
        GameObjectConversionSettings _convSettings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        carPrefabs = new NativeArray<Entity>(_carPrefabs.Count, Allocator.Persistent);

        for (int i = 0; i < _carPrefabs.Count; i++)
        {
            carPrefabs[i] = GameObjectConversionUtility.ConvertGameObjectHierarchy(_carPrefabs[i], _convSettings);
        }
    }

    void OnDestroy()
    {
        if (carPrefabs.IsCreated)
            carPrefabs.Dispose();
    }
}
