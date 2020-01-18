using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class TrafficControlBlockAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [Header("Node Groups")]
    [SerializeField] private List<ControlGroup> _groups;

    [Header("State Masks")]
    [SerializeField] private List<ControlState> _stateMasks;

    [Header("Start Setup")]
    [SerializeField] private int _startStateId;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        //start check
        foreach (var state in _stateMasks)
        {
            if (state.mask.Count != _groups.Count)
                throw new System.Exception(gameObject.name + ": Traffic Control Mask value count is not equal to groups count");

            if (_startStateId >= _stateMasks.Count)
                throw new System.Exception(gameObject.name + ": Start state ID is out of bounds");
        }

        //convert
        var _manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //add components
        _manager.AddComponent(entity, typeof(TrafficControlBlockInitComponent));
        _manager.AddComponentData<TrafficControlBlockComponent>(entity, new TrafficControlBlockComponent { groupsCount = _groups.Count, statesCount = _stateMasks.Count });
        _manager.AddComponentData<TrafficControlStateComponent>(entity, new TrafficControlStateComponent { stateId = _startStateId, stateRemainingTime = _stateMasks[_startStateId].stateLifetime });
        //add buffers
        var _groupsBuffer = _manager.AddBuffer<NodeBufferElement>(entity);
        var _groupStartIdsBuffer = _manager.AddBuffer<IntBufferElement>(entity);
        int _startCounter = 0;
        _groupStartIdsBuffer.Add(new IntBufferElement { value = _startCounter });
        //add groups
        for (int i = 0; i < _groups.Count; i++)
        {
            _startCounter += _groups[i].groupNodes.Count;
            _groupStartIdsBuffer.Add(new IntBufferElement { value = _startCounter });

            for (int j = 0; j < _groups[i].groupNodes.Count; j++)
            {
                _groupsBuffer.Add(new NodeBufferElement { node = conversionSystem.GetPrimaryEntity(_groups[i].groupNodes[j]) });
            }
        }
        //add states
        var _statesBuffer = _manager.AddBuffer<BoolBufferElement>(entity);
        for (int i = 0; i < _stateMasks.Count; i++)
        {
            for (int j = 0; j < _stateMasks[i].mask.Count; j++)
            {
                _statesBuffer.Add(new BoolBufferElement { value = _stateMasks[i].mask[i] });
            }
        }
    }

    [System.Serializable]
    private struct ControlGroup
    {
        public string groupName;
        public List<GameObject> groupNodes;
    }

    [System.Serializable]
    private class ControlState
    {
        public List<bool> mask;
        public int stateLifetime;
    }
}
