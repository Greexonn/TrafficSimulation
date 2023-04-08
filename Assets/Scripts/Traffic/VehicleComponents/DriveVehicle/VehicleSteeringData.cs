using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Traffic.VehicleComponents.DriveVehicle
{
    public struct VehicleSteeringData : IComponentData
    {
        public float MaxAngle;
        public float SteeringSpeed;

        [HideInInspector] public quaternion CurrentRotation;
        [HideInInspector] public float TargetRotationAngle;
        public float CurrentTransition;
        [HideInInspector] public int Direction;
    }
}
