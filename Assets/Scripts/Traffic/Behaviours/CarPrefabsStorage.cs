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

    private BlobAssetStore _assetStore;

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
        _assetStore = new BlobAssetStore();

        GameObjectConversionSettings _convSettings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, _assetStore);
        carPrefabs = new NativeArray<Entity>(_carPrefabs.Count, Allocator.Persistent);

        for (int i = 0; i < _carPrefabs.Count; i++)
        {
            carPrefabs[i] = GameObjectConversionUtility.ConvertGameObjectHierarchy(_carPrefabs[i], _convSettings);
        }
    }

    void OnDestroy()
    {
        _assetStore?.Dispose();

        if (carPrefabs.IsCreated)
            carPrefabs.Dispose();
    }
}
