using Unity.Entities;
using UnityEngine;

namespace TrafficSimulation.Traffic.RoadComponents
{
    [DisallowMultipleComponent]
    public class RoadNodeAuthoring : MonoBehaviour
    {
        public void Convert(Entity entity, EntityManager dstManager)
        {
            var nodeComponent = new RoadNodeData
            {
                IsOpen = true
            };

            dstManager.AddComponentData(entity, nodeComponent);
        }
    }
}
