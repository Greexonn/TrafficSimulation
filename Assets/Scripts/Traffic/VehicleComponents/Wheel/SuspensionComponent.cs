using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct SuspensionComponent : IComponentData
{
    public float suspensionLength;
    public float springStrength;
    public float damperStrength;
}
