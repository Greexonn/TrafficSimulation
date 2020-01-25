using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
[RequireComponent(typeof(VehicleAuthoring))]
public class VehicleSteeringAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private VehicleSteeringComponent _steering;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddSharedComponentData<VehicleSteeringComponent>(entity, _steering);

        var _steeringWheels = GetComponent<VehicleAuthoring>().steeringWheels;

        foreach (var wheel in _steeringWheels)
        {
            var _wheelEntity = conversionSystem.GetPrimaryEntity(wheel.gameObject);
            dstManager.AddSharedComponentData<VehicleSteeringComponent>(_wheelEntity, _steering);
        }
    }
}
