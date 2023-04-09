using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace TrafficSimulation.Traffic.RoadComponents
{
    [DisallowMultipleComponent]
    public class RoadBlock : MonoBehaviour
    {
        [SerializeField] private List<RoadBlock> _connectedBlocks;

        [SerializeField] private List<Transform> _exits;
        [SerializeField] public List<Transform> enters;

        [SerializeField] private List<RoadLine> _lines;

        //
        [Header("Gizmos")]
        [SerializeField] private Mesh _arrow;

        public void Bake(IBaker baker, NativeParallelMultiHashMap<Entity, Entity> chunkGraph)
        {
            for (var i = 0; i < _lines.Count; i++)
            {
                var aEntity = baker.GetEntity(_lines[i].A.gameObject, TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);
                var bEntity = baker.GetEntity(_lines[i].B.gameObject, TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);
                chunkGraph.Add(aEntity, bEntity);
            }
        }

        public int GetLinesCount()
        {
            return _lines?.Count ?? 0;
        }

        public void ConnectNodes()
        {
            foreach (var exit in _exits)
            {
                var exitNode = exit.GetComponent<RoadNodeAuthoring>();

                foreach (var block in _connectedBlocks)
                {
                    foreach (var enter in block.enters)
                    {
                        if (!(Vector3.Distance(exit.position, enter.position) < 0.2f)) 
                            continue;
                        
                        var enterNode = enter.GetComponent<RoadNodeAuthoring>();

                        for (var i = 0; i < _lines.Count; i++)
                        {
                            if (_lines[i].B != exitNode) 
                                continue;
                            
                            var line = _lines[i];
                            line.B = enterNode;
                            _lines[i] = line;
                        }
                    }
                }
            }
        }
    
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;

            foreach (var line in _lines)
            {
                if (line.A == null || line.B == null)
                    return;

                var positionA = line.A.transform.position;
                var positionB = line.B.transform.position;
                var lineDir = (positionB - positionA).normalized;
                var delta = lineDir * 0.05f;
                var arrowRot = Quaternion.LookRotation(lineDir);
                //draw line
                Gizmos.DrawLine(positionA, positionB);
                //draw arrows
                Gizmos.color = new Color(0, 255, 0, 0.7f);
                Gizmos.DrawMesh(_arrow, positionA + delta, arrowRot, new Vector3(3, 3, 3));
                Gizmos.DrawMesh(_arrow, positionB + delta, arrowRot, new Vector3(3, 3, 3));
                //draw node names
                Gizmos.color = Color.white;
                UnityEditor.Handles.Label(positionA - delta, line.A.gameObject.name);
                UnityEditor.Handles.Label(positionB + delta, line.B.gameObject.name);
            }
        }
#endif

        [Serializable]
        public struct RoadLine
        {
            public RoadNodeAuthoring A, B;
        }
    }
}
