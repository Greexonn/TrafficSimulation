using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class RoadNodeAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        RoadNodeComponent _nodeComponent = new RoadNodeComponent
        {
            isOpen = true
        };

        dstManager.AddComponentData(entity, _nodeComponent);
    }
}
