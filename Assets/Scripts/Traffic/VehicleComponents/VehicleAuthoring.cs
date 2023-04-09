using System.Collections.Generic;
using TrafficSimulation.Traffic.VehicleComponents.Wheel;
using UnityEngine;

namespace TrafficSimulation.Traffic.VehicleComponents
{
    [DisallowMultipleComponent]
    public class VehicleAuthoring : MonoBehaviour
    {
        [SerializeField] public List<WheelAuthoring> _wheels;
        [SerializeField] public List<WheelAuthoring> _driveWheels;
        [SerializeField] public List<WheelAuthoring> _controlWheels;
        [SerializeField] public List<WheelAuthoring> _breakWheels;
    }
}
