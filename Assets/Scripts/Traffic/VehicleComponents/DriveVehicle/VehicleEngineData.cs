using Unity.Entities;
using UnityEngine;

namespace Traffic.VehicleComponents.DriveVehicle
{
    public struct VehicleEngineData : IComponentData
    {
        public int MaxSpeed;

        [HideInInspector] public float CurrentSpeed;
        [HideInInspector] public int Acceleration;
    }
}
