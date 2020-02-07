using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct VehicleAIComponent : IComponentData
{
    public Entity vehicleAITransform;
}
