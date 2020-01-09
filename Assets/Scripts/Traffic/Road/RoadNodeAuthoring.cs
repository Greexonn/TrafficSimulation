using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class RoadNodeAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private RoadNodeAuthoring _nextNode;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        RoadNodeComponent _nodeComponent = new RoadNodeComponent
        {
            nextNode = _nextNode != null ? conversionSystem.GetPrimaryEntity(_nextNode.gameObject) : Entity.Null,
            isAvalible = true
        };

        dstManager.AddComponentData(entity, _nodeComponent);
    }
}
