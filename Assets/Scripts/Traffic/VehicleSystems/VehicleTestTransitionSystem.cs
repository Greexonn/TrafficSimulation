using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class VehicleTestTransitionSystem : ComponentSystem
{
    private EntityManager _manager;

    protected override void OnCreate()
    {
        _manager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    protected override void OnUpdate()
    {
        float _deltaTime = Time.DeltaTime;

        Entities.WithAll(typeof(VehicleComponent)).WithNone(typeof(PathfindingRequestComponent), typeof(VehicleAIComponent)).ForEach((Entity vehicleEntity, ref Translation translation, ref VehicleCurrentNodeComponent currentNode, ref VehiclePathNodeIndexComponent pathNodeIndex) => 
        {
            var _pathBuffer = _manager.GetBuffer<NodeBufferElement>(vehicleEntity);

            pathNodeIndex.value = math.clamp(pathNodeIndex.value, 0, (_pathBuffer.Length - 1));
            int _nextNodeId = math.clamp((pathNodeIndex.value + 1), 0, (_pathBuffer.Length - 1));
            var _currentNodePos = _manager.GetComponentData<LocalToWorld>(currentNode.node).Position;
            var _nextNodePos = _manager.GetComponentData<LocalToWorld>(_pathBuffer[_nextNodeId].node).Position;

            if (currentNode.node.Equals(_pathBuffer[_pathBuffer.Length - 1].node))
            {
                // _manager.RemoveComponent(vehicleEntity, typeof(VehicleComponent));
                _manager.AddComponent(vehicleEntity, typeof(PathfindingRequestComponent));
                return;
            }

            //check node reach
            float _distanceToNextNode = math.distance(translation.Value, _nextNodePos);

            if (_distanceToNextNode < 0.01f)
            {
                pathNodeIndex.value = math.clamp((pathNodeIndex.value + 1), 0, (_pathBuffer.Length - 1));
                if (_manager.GetComponentData<RoadNodeComponent>(_pathBuffer[pathNodeIndex.value].node).isOpen)
                    currentNode.node = _pathBuffer[pathNodeIndex.value].node;
                else
                    pathNodeIndex.value--;

                return;
            }

            //move
            float3 _moveDirection = math.normalizesafe(_nextNodePos - translation.Value);
            translation.Value += _moveDirection * _deltaTime;
        });
    }
}