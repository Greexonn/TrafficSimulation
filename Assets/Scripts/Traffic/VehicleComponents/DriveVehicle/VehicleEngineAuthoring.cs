using Unity.Entities;
using UnityEngine;

namespace TrafficSimulation.Traffic.VehicleComponents.DriveVehicle
{
    public class VehicleEngineAuthoring : MonoBehaviour
    {
        public int maxSpeed;
    }
    
    public struct VehicleEngineData : IComponentData
    {
        public int MaxSpeed;

        [HideInInspector] public float CurrentSpeed;
        [HideInInspector] public int Acceleration;
    }
}
