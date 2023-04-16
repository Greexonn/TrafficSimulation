using System;
using Unity.Entities;
using UnityEngine;

namespace TrafficSimulation.Traffic.VehicleComponents.Wheel
{
    [Serializable]
    public struct SuspensionData : IComponentData
    {
        public float suspensionLength;
        [HideInInspector] public float springStrength;
        [HideInInspector] public float damperStrength;
    }
}
