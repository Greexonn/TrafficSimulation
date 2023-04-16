using Unity.Entities;
using UnityEngine;

namespace TrafficSimulation.Traffic.VehicleComponents.DriveVehicle
{
    public class VehicleBrakesAuthoring : MonoBehaviour
    {}
    
    public struct VehicleBrakesData : IComponentData
    {
        public int BrakesUsage;
    }
}
