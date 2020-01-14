using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class CarSpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private RoadNodeAuthoring _spawnNode;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(conversionSystem.GetPrimaryEntity(_spawnNode.gameObject), typeof(CarSpawnerComponent));
    }
}
