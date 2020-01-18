using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct TrafficControlStateComponent : IComponentData
{
    public int stateId;
    public float stateRemainingTime;
}
