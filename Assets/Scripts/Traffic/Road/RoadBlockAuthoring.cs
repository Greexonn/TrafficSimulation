using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class RoadBlockAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [HideInInspector] public RoadChunkAuthoring parentChunk;

    [SerializeField] private RoadLine[] _lines;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (parentChunk != null)
        {
            for (int i = 0; i < _lines.Length; i++)
            {
                parentChunk.chunkGraph.Add(conversionSystem.GetPrimaryEntity(_lines[i].A.gameObject), conversionSystem.GetPrimaryEntity(_lines[i].B.gameObject));
            }
        }
    }

    public int GetLinesCount()
    {
        if (_lines != null)
            return _lines.Length;
        else
            return 0;
    }

    [System.Serializable]
    public struct RoadLine
    {
        public RoadNodeAuthoring A, B;
    }
}
