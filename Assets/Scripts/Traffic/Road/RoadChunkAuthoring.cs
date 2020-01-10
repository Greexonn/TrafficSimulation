using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class RoadChunkAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public NativeMultiHashMap<Entity, Entity> chunkGraph;

    void Awake()
    {
        var _roadBlocks = GetComponentsInChildren<RoadBlockAuthoring>();

        int _linesCount = 0;

        for (int i = 0; i < _roadBlocks.Length; i++)
        {
            _roadBlocks[i].parentChunk = this;

            _linesCount += _roadBlocks[i].GetLinesCount();
        }

        if (_linesCount > 0)
        {
            chunkGraph = new NativeMultiHashMap<Entity, Entity>(_linesCount, Allocator.Persistent);
        }
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {       
        TrafficSystem.instance.graphs.Add(chunkGraph);


    }
}
