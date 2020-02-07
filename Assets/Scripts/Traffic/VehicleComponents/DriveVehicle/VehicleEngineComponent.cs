using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct VehicleEngineComponent : IComponentData
{
    public int maxSpeed;

    [UnityEngine.HideInInspector] public float currentSpeed;
    [UnityEngine.HideInInspector] public int acceleration;
}
