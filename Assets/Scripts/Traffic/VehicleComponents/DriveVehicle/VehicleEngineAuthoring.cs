using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
[RequireComponent(typeof(VehicleEngineAuthoring))]
public class VehicleEngineAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private VehicleEngineComponent _engine;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddSharedComponentData<VehicleEngineComponent>(entity, _engine);

        var _driveWheels = GetComponent<VehicleAuthoring>().driveWheels;

        foreach (var wheel in _driveWheels)
        {
            var _wheelEntity = conversionSystem.GetPrimaryEntity(wheel.gameObject);
            dstManager.AddSharedComponentData<VehicleEngineComponent>(_wheelEntity, _engine);
        }
    }
}
