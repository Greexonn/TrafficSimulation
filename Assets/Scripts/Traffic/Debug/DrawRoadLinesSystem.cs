using Traffic.RoadComponents;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Traffic.Debug
{
    public partial class DrawRoadLinesSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (TrafficSystem.Instance == null) 
                return;
            
            var graphs = TrafficSystem.Instance.Graphs;
            for (var i = 0; i < graphs.Count; i++)
            {
                var keys = graphs[i].GetKeyArray(Allocator.Temp);

                foreach (var key in keys)
                {
                    var values = graphs[i].GetValuesForKey(key);

                    var keyLocalToWorld = EntityManager.GetComponentData<LocalToWorld>(key);

                    var keyOpen = EntityManager.GetComponentData<RoadNodeData>(key).isOpen;
                    var lineColor = !keyOpen ? Color.red : Color.green;

                    foreach (var node in values)
                    {
                        var valueLocalToWorld = EntityManager.GetComponentData<LocalToWorld>(node);

                        UnityEngine.Debug.DrawLine(keyLocalToWorld.Position, valueLocalToWorld.Position, lineColor);
                    }
                }

                //dispose temporal
                keys.Dispose();
            }
        }
    }
}