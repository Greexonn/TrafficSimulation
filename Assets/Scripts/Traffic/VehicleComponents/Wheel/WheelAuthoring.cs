using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class WheelAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] public WheelComponent wheel;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var _wheelModel = GetComponent<SuspensionAuthoring>().wheelModel;
        wheel.wheelModel = conversionSystem.GetPrimaryEntity(_wheelModel);
        wheel.wheelPosition = _wheelModel.position;

        dstManager.AddComponentData<WheelComponent>(entity, wheel);
    }
}
