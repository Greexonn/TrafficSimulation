using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct VehicleEngineComponent : ISharedComponentData
{
    public int maxSpeed;
    public float acceleration;
}
