using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class RoadBlockAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private List<RoadBlockAuthoring> _connectedBlocks;

    [SerializeField] private List<Transform> _exits;
    [SerializeField] public List<Transform> enters;

    [HideInInspector] public RoadChunkAuthoring parentChunk;

    [SerializeField] private List<RoadLine> _lines;

    void Awake()
    {
        ConnectNodes();
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (parentChunk != null)
        {
            for (int i = 0; i < _lines.Count; i++)
            {
                parentChunk.chunkGraph.Add(conversionSystem.GetPrimaryEntity(_lines[i].A.gameObject), conversionSystem.GetPrimaryEntity(_lines[i].B.gameObject));
            }
        }
    }

    public int GetLinesCount()
    {
        if (_lines != null)
            return _lines.Count;
        else
            return 0;
    }

    private void ConnectNodes()
    {
        foreach (var block in _connectedBlocks)
        {
            foreach (var exit in _exits)
            {
                foreach (var enter in block.enters)
                {
                    if (Vector3.Distance(exit.position, enter.position) < 0.2f)
                    {
                        _lines.Add(new RoadLine
                        {
                            A = exit.gameObject.GetComponent<RoadNodeAuthoring>(),
                            B = enter.gameObject.GetComponent<RoadNodeAuthoring>()
                        });
                    }
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        foreach (var line in _lines)
        {
            Gizmos.DrawLine(line.A.transform.position, line.B.transform.position);
        }
    }

    [System.Serializable]
    public struct RoadLine
    {
        public RoadNodeAuthoring A, B;
    }
}
