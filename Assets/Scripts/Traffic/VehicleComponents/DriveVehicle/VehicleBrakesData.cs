using Unity.Entities;
using UnityEngine;

namespace Traffic.VehicleComponents.DriveVehicle
{
    public struct VehicleBrakesData : IComponentData
    {
        [HideInInspector] public int BrakesUsage;
    }
}
