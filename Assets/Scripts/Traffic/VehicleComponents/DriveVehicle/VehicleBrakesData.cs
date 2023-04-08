using Unity.Entities;
using UnityEngine;

namespace TrafficSimulation.Traffic.VehicleComponents.DriveVehicle
{
    public struct VehicleBrakesData : IComponentData
    {
        [HideInInspector] public int BrakesUsage;
    }
}
