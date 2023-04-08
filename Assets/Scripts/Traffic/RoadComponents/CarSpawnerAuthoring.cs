using Unity.Entities;
using UnityEngine;

namespace Traffic.RoadComponents
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
