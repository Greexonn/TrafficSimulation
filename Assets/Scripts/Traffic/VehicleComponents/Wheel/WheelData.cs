using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Traffic.VehicleComponents.Wheel
{
    [Serializable]
    public struct WheelData : IComponentData
    {
        public float radius;
        [Header("Friction")]
        public float sideFriction;
        public float forwardFriction;
        public float maxSideFriction;
        public float maxForwardFriction;
    
        public Entity wheelModel;
        [HideInInspector] public float3 wheelPosition;
    }
}
