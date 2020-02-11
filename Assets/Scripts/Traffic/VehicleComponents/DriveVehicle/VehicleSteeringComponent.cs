using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct VehicleSteeringComponent : IComponentData
{
    public float maxAngle;
    public float steeringSpeed;

    [UnityEngine.HideInInspector] public quaternion currentRotation;
    [UnityEngine.HideInInspector] public float targetRotationAngle;
    public float currentTransition;
    [UnityEngine.HideInInspector] public int direction;
}
