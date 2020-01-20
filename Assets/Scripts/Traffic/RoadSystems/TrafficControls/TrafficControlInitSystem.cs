using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class TrafficControlInitSystem : ComponentSystem
{
    private EntityManager _manager;

    protected override void OnCreate()
    {
        _manager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    protected override void OnUpdate()
    {
        float _deltaTime = Time.DeltaTime;

        Entities.WithAll(typeof(TrafficControlBlockInitComponent)).ForEach((Entity blockEntity, ref TrafficControlBlockComponent controlBlock, ref TrafficControlStateComponent controlState) =>
        {
            var _groupsBuffer = _manager.GetBuffer<NodeBufferElement>(blockEntity);
            var _groupStartIdsBuffer = _manager.GetBuffer<StartIDsBufferElement>(blockEntity);
            var _statesBuffer = _manager.GetBuffer<TCStateBufferElement>(blockEntity);

            int _stateId = controlState.stateId;
            int _stateOffset = _stateId * controlBlock.groupsCount;

            for (int i = 0; i < controlBlock.groupsCount; i++)
            {
                bool _groupState = _statesBuffer[_stateOffset + i].value;
                //apply state to group
                int _groupStartId = _groupStartIdsBuffer[i].value;
                int _groupEndId = _groupStartIdsBuffer[i + 1].value;
                for (int g = _groupStartId; g < _groupEndId; g++)
                {
                    _manager.SetComponentData<RoadNodeComponent>(_groupsBuffer[g].node, new RoadNodeComponent { isOpen = _groupState });
                }
            }

            //remove init component
            _manager.RemoveComponent(blockEntity, typeof(TrafficControlBlockInitComponent));
        });
    }
}