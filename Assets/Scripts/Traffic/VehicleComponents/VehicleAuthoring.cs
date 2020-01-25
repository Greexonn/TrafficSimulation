using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class VehicleAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] public List<WheelAuthoring> driveWheels;
    [SerializeField] public List<WheelAuthoring> steeringWheels;
    [SerializeField] public List<WheelAuthoring> brakeWheels;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(VehicleComponent));
    }
}
