using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct WheelComponent : IComponentData
{
    public float wheelRadius;
    [Header("Friction")]
    public float sideFriction;
    public float forwardFriction;
    public float maxSideFriction;
    public float maxForwardFriction;
}
