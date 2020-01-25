using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct VehicleSteeringComponent : ISharedComponentData
{
    public float currentAngle;
    public float maxAngle;
    public float steeringSpeed;
}
