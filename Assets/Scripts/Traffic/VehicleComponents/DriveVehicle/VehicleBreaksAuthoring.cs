using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
[RequireComponent(typeof(VehicleAuthoring))]
public class VehicleBreaksAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    private VehicleBreaksComponent _breaks;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddSharedComponentData<VehicleBreaksComponent>(entity, _breaks);

        var _breakWheels = GetComponent<VehicleAuthoring>().breakWheels;

        foreach (var wheel in _breakWheels)
        {
            var _wheelEntity = conversionSystem.GetPrimaryEntity(wheel.gameObject);

            dstManager.AddSharedComponentData<VehicleBreaksComponent>(_wheelEntity, _breaks);
        }
    }
}
