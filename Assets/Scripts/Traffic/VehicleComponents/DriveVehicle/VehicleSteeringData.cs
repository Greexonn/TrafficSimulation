using Unity.Entities;
using Unity.Mathematics;

namespace Traffic.VehicleComponents.DriveVehicle
{
    [GenerateAuthoringComponent]
    public struct VehicleSteeringData : IComponentData
    {
        public float maxAngle;
        public float steeringSpeed;

        [UnityEngine.HideInInspector] public quaternion currentRotation;
        [UnityEngine.HideInInspector] public float targetRotationAngle;
        public float currentTransition;
        [UnityEngine.HideInInspector] public int direction;
    }
}
