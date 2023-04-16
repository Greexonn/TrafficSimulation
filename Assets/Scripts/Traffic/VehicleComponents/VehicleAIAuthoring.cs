using Unity.Entities;
using UnityEngine;

namespace TrafficSimulation.Traffic.VehicleComponents
{
    public class VehicleAIAuthoring : MonoBehaviour
    {
        public GameObject vehicleAITransform;
    }
    
    public struct VehicleAIData : IComponentData
    {
        public Entity VehicleAITransform;
    }
}
