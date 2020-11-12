using Unity.Entities;

namespace Traffic.VehicleComponents.DriveVehicle
{
    [GenerateAuthoringComponent]
    public struct VehicleEngineData : IComponentData
    {
        public int maxSpeed;

        [UnityEngine.HideInInspector] public float currentSpeed;
        [UnityEngine.HideInInspector] public int acceleration;
    }
}
