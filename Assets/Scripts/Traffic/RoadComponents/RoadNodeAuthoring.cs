using Unity.Entities;
using UnityEngine;

namespace Traffic.RoadComponents
{
    [DisallowMultipleComponent]
    public class RoadNodeAuthoring : MonoBehaviour
    {
        public void Convert(Entity entity, EntityManager dstManager)
        {
            var nodeComponent = new RoadNodeData
            {
                isOpen = true
            };

            dstManager.AddComponentData(entity, nodeComponent);
        }
    }
}
