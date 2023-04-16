using System.Collections.Generic;
using UnityEngine;

namespace TrafficSimulation.Traffic.Behaviours
{
    public class CarPrefabsStorageAuthoring : MonoBehaviour
    {
        [SerializeField] public List<GameObject> _carPrefabs;
    }
}
