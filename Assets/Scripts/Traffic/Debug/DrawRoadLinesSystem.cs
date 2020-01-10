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
        if (TrafficSystem.instance != null)
        {
            var _graphs = TrafficSystem.instance.graphs;
            for (int i = 0; i < _graphs.Count; i++)
            {
                var _keys = _graphs[i].GetKeyArray(Allocator.Temp);

                for (int k = 0; k < _keys.Length; k++)
                {
                    var _values = _graphs[i].GetValuesForKey(_keys[k]);

                    var _keyLocalToWorld = _manager.GetComponentData<LocalToWorld>(_keys[k]);

                    foreach (var node in _values)
                    {
                        var _valueLocalToWorld = _manager.GetComponentData<LocalToWorld>(node);

                        Debug.DrawLine(_keyLocalToWorld.Position, _valueLocalToWorld.Position, Color.green);
                    }
                }

                //dispose temporals
                _keys.Dispose();
            }
        }
    }
}