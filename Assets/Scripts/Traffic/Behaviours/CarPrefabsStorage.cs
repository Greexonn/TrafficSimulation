using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace TrafficSimulation.Traffic.Behaviours
{
    public class CarPrefabsStorage : MonoBehaviour
    {
        public static CarPrefabsStorage Instance;

        [SerializeField] private List<GameObject> _carPrefabs;

        public NativeArray<Entity> CarPrefabs;

        private BlobAssetStore _assetStore;

        private void Awake()
        {
            if (Instance == this)
                return;
            
            if (Instance != null)
                Destroy(this);
            else
                Instance = this;
        }

        void Start()
        {
            _assetStore = new BlobAssetStore();

            // GameObjectConversionSettings _convSettings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, _assetStore);
            CarPrefabs = new NativeArray<Entity>(_carPrefabs.Count, Allocator.Persistent);

            for (var i = 0; i < _carPrefabs.Count; i++)
            {
                // carPrefabs[i] = GameObjectConversionUtility.ConvertGameObjectHierarchy(_carPrefabs[i], _convSettings);
            }
        }

        private void OnDestroy()
        {
            // _assetStore?.Dispose();

            if (CarPrefabs.IsCreated)
                CarPrefabs.Dispose();
        }
    }
}
