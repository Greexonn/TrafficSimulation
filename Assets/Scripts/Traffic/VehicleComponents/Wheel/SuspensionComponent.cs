using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct SuspensionComponent : IComponentData
{
    public float suspensionLength;
    [HideInInspector] public float springStrength;
    [HideInInspector] public float damperStrength;
}
