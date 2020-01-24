using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct SuspensionComponent : IComponentData
{
    public float suspensionLength;
    [Range(0, 1)]
    public float wheelPosition;
    public float springStrength;
    public float damperStrength;
}
