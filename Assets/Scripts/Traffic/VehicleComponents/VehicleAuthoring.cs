using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class VehicleAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private List<WheelAuthoring> _wheels;
    [SerializeField] private List<WheelAuthoring> _driveWheels;
    [SerializeField] private List<WheelAuthoring> _controlWheels;
    [SerializeField] private List<WheelAuthoring> _breakWheels;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(VehicleComponent));

        //add wheel buffers
        dstManager.AddBuffer<IWheelBufferComponent>(entity);
        dstManager.AddBuffer<IDriveWheelBufferComponent>(entity);
        dstManager.AddBuffer<IControlWheelBufferComponent>(entity);
        dstManager.AddBuffer<IBrakeWheelBufferComponent>(entity);
        //get buffers
        var _wheelBuffer = dstManager.GetBuffer<IWheelBufferComponent>(entity);
        var _driveWheelBuffer = dstManager.GetBuffer<IDriveWheelBufferComponent>(entity);
        var _controlWheelBuffer = dstManager.GetBuffer<IControlWheelBufferComponent>(entity);
        var _breakWheelBuffer = dstManager.GetBuffer<IBrakeWheelBufferComponent>(entity);
        //fill buffers
        foreach (var wheel in _wheels)
        {
            _wheelBuffer.Add(new IWheelBufferComponent { wheel = conversionSystem.GetPrimaryEntity(wheel) });
        }
        foreach (var wheel in _driveWheels)
        {
            _driveWheelBuffer.Add(new IDriveWheelBufferComponent { wheelID = _wheels.IndexOf(wheel) });
        }
        foreach (var wheel in _controlWheels)
        {
            _controlWheelBuffer.Add(new IControlWheelBufferComponent { wheelID = _wheels.IndexOf(wheel) });
        }
        foreach (var wheel in _breakWheels)
        {
            _breakWheelBuffer.Add(new IBrakeWheelBufferComponent { wheelID = _wheels.IndexOf(wheel) });
        }
    }
}
