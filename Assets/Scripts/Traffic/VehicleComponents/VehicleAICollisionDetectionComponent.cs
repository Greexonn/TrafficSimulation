using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct VehicleAICollisionDetectionComponent : IComponentData
{
    public Entity leftRayPoint, rightRayPoint;
}
