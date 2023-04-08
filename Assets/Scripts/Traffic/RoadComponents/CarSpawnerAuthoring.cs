using Unity.Entities;
using UnityEngine;

namespace TrafficSimulation.Traffic.RoadComponents
{
    [DisallowMultipleComponent]
    public class CarSpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] private RoadNodeAuthoring _spawnNode;

        public void Convert(Entity entity, EntityManager dstManager)
        {
            // dstManager.AddComponent(conversionSystem.GetPrimaryEntity(_spawnNode.gameObject), typeof(CarSpawnerComponent));
        }
    }
}
