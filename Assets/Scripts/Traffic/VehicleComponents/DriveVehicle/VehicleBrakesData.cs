using Unity.Entities;

namespace Traffic.VehicleComponents.DriveVehicle
{
    [GenerateAuthoringComponent]
    public struct VehicleBrakesData : IComponentData
    {
        [UnityEngine.HideInInspector] public int brakesUsage;
    }
}
