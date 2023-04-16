using TrafficSimulation.Traffic.RoadComponents;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace TrafficSimulation.Traffic.Debug
{
    public partial class DrawRoadLinesSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<TrafficSystemData>();
        }

        protected override void OnUpdate()
        {
            var trafficSystemData = SystemAPI.GetSingleton<TrafficSystemData>();
            
            var graphs = trafficSystemData.Graphs;
            for (var i = 0; i < graphs.Length; i++)
            {
                var keys = graphs[i].GetKeyArray(Allocator.Temp);

                foreach (var key in keys)
                {
                    var values = graphs[i].GetValuesForKey(key);
                    
                    var keyLocalToWorld = SystemAPI.GetComponent<LocalToWorld>(key);

                    var keyOpen = SystemAPI.GetComponent<RoadNodeData>(key).IsOpen;
                    var lineColor = !keyOpen ? Color.red : Color.green;

                    foreach (var node in values)
                    {
                        var valueLocalToWorld = SystemAPI.GetComponent<LocalToWorld>(node);

                        UnityEngine.Debug.DrawLine(keyLocalToWorld.Position, valueLocalToWorld.Position, lineColor);
                    }
                }

                //dispose temporal
                keys.Dispose();
            }
        }
    }
}