using Unity.Entities;
using UnityEngine;

namespace Traffic.RoadComponents
{
    [DisallowMultipleComponent]
    public class RoadNodeAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var nodeComponent = new RoadNodeData
            {
                isOpen = true
            };

            dstManager.AddComponentData(entity, nodeComponent);
        }
    }
}
