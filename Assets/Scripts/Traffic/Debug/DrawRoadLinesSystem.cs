using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class DrawRoadLinesSystem : ComponentSystem
{
    private EntityManager _manager;

    protected override void OnCreate()
    {
        _manager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    protected override void OnUpdate()
    {
        Entities.WithAll<RoadNodeComponent>().ForEach((ref RoadNodeComponent node, ref LocalToWorld localToWorld) =>
        {
            if (node.nextNode != Entity.Null)
            {
                var _nextNodeData = _manager.GetComponentData<RoadNodeComponent>(node.nextNode);
                var _nextNodeLocalToWorld = _manager.GetComponentData<LocalToWorld>(node.nextNode);

                Color _lineColor = _nextNodeData.isAvalible ? Color.green : Color.red;

                Debug.DrawLine(localToWorld.Position, _nextNodeLocalToWorld.Position, _lineColor);
            }
        });
    }
}