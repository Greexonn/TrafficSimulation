using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct VehicleSteeringComponent : IComponentData
{
    public float currentAngle;
    public float maxAngle;
    public float steeringSpeed;
}
