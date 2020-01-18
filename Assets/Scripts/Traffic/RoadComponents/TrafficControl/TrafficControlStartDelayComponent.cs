using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct TrafficControlStartDelayComponent : IComponentData
{
    public float value;
}
