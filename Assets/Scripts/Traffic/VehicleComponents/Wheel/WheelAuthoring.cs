using UnityEngine;
using UnityEngine.Serialization;

namespace TrafficSimulation.Traffic.VehicleComponents.Wheel
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SuspensionAuthoring))]
    public class WheelAuthoring : MonoBehaviour
    {
        [FormerlySerializedAs("_wheel")]
        [SerializeField] public WheelData wheel;
    }
}
