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
        wheel.wheelModel = conversionSystem.GetPrimaryEntity(GetComponent<SuspensionAuthoring>().wheelModel);
        wheel.vehiclePhysicsBody = conversionSystem.GetPrimaryEntity(GetComponentInParent<Unity.Physics.Authoring.PhysicsBodyAuthoring>().gameObject);

        dstManager.AddComponentData<WheelComponent>(entity, wheel);
    }
}
