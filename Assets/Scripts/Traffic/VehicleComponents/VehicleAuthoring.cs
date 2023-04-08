using System.Collections.Generic;
using TrafficSimulation.Traffic.VehicleComponents.Wheel;
using Unity.Entities;
using UnityEngine;

namespace TrafficSimulation.Traffic.VehicleComponents
{
    [DisallowMultipleComponent]
    public class VehicleAuthoring : MonoBehaviour
    {
        [SerializeField] private List<WheelAuthoring> _wheels;
        [SerializeField] private List<WheelAuthoring> _driveWheels;
        [SerializeField] private List<WheelAuthoring> _controlWheels;
        [SerializeField] private List<WheelAuthoring> _breakWheels;

        public void Convert(Entity entity, EntityManager dstManager)
        {
            dstManager.AddComponent(entity, typeof(VehicleTag));

            //add wheel buffers
            dstManager.AddBuffer<WheelElement>(entity);
            dstManager.AddBuffer<DriveWheelElement>(entity);
            dstManager.AddBuffer<ControlWheelElement>(entity);
            dstManager.AddBuffer<BrakeWheelElement>(entity);
            
            
            var wheelBuffer = dstManager.GetBuffer<WheelElement>(entity);
            
            //fill buffers
            // foreach (var wheelEntity in _wheels.Select(conversionSystem.GetPrimaryEntity))
            // {
            //     wheelBuffer.Add(new WheelElement { wheel = wheelEntity });
            // }
            
            var driveWheelBuffer = dstManager.GetBuffer<DriveWheelElement>(entity);
            foreach (var wheel in _driveWheels)
            {
                driveWheelBuffer.Add(new DriveWheelElement { WheelID = _wheels.IndexOf(wheel) });
            }
            
            var controlWheelBuffer = dstManager.GetBuffer<ControlWheelElement>(entity);
            foreach (var wheel in _controlWheels)
            {
                controlWheelBuffer.Add(new ControlWheelElement { WheelID = _wheels.IndexOf(wheel) });
            }
            
            var breakWheelBuffer = dstManager.GetBuffer<BrakeWheelElement>(entity);
            foreach (var wheel in _breakWheels)
            {
                breakWheelBuffer.Add(new BrakeWheelElement { WheelID = _wheels.IndexOf(wheel) });
            }

#if UNITY_EDITOR
            dstManager.SetName(entity, $"{gameObject.name}");
#endif
        }
    }
}
