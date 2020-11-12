using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Traffic.VehicleComponents.Wheel
{
    [Serializable]
    public struct WheelRaycastData : IComponentData
    {
        public bool IsHitThisFrame;

        public float3 VelocityAtWheel;
        public float3 HitPosition;
        public int HitRBIndex;
    }
}
