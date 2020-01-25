using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct VehicleBreaksComponent : IComponentData
{
    public float breaksUsage;
}
