using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct VehiclePathNodeIndexComponent : IComponentData
{
    public int value;
}
