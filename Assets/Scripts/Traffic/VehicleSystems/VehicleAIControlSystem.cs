using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static UnityEngine.Debug;

public class VehicleAIControlSystem : ComponentSystem
{
    private EntityManager _manager;

    private float3 _mapForward;
    private float3 _mapRight;

    protected override void OnCreate()
    {
        _manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        _mapForward = new float3(0, 0, 1);
        _mapRight = new float3(1, 0, 0);
    }

    protected override void OnUpdate()
    {
        float _deltaTime = Time.DeltaTime;

        Entities.WithAll(typeof(VehicleComponent)).WithNone(typeof(PathfindingRequestComponent)).
            ForEach((Entity vehicleEntity, ref VehicleAIComponent aiComponent, ref VehicleCurrentNodeComponent currentNode, ref VehiclePathNodeIndexComponent pathNodeIndex,
            ref VehicleSteeringComponent steering, ref VehicleEngineComponent engine) =>
        {
            var _aiTransforms = _manager.GetComponentData<LocalToWorld>(aiComponent.vehicleAITransform);
            var _aiPosition = _aiTransforms.Position;
            var _aiUp = _aiTransforms.Up;
            var _aiForwrd = _aiTransforms.Forward;

            var _pathBuffer = _manager.GetBuffer<NodeBufferElement>(vehicleEntity).Reinterpret<Entity>();

            //get nodes data
            pathNodeIndex.value = math.clamp(pathNodeIndex.value, 0, (_pathBuffer.Length - 1));
            int _nextNodeId = math.clamp((pathNodeIndex.value + 1), 0, (_pathBuffer.Length - 1));
            var _currentNodePos = _manager.GetComponentData<LocalToWorld>(currentNode.node).Position;
            var _nextNodePos = _manager.GetComponentData<LocalToWorld>(_pathBuffer[_nextNodeId]).Position;

            //check if we've reached our target
            if (currentNode.node.Equals(_pathBuffer[_pathBuffer.Length - 1]))
            {
                _manager.AddComponent(vehicleEntity, typeof(PathfindingRequestComponent));
                return;
            }

            //check if we've reached current node
            float _mapX = math.dot(_aiPosition, _mapRight);
            float _mapY = math.dot(_aiPosition, _mapForward);
            var _aiMapPos = new float2(_mapX, _mapY);
            _mapX = math.dot(_nextNodePos, _mapRight);
            _mapY = math.dot(_nextNodePos, _mapForward);
            var _nodeMapPos = new float2(_mapX, _mapY);

            float _distanceToNextNode = math.distance(_aiMapPos, _nodeMapPos);
            //get next node if reached
            if (_distanceToNextNode < (_deltaTime * 20))
            {
                pathNodeIndex.value = math.clamp((pathNodeIndex.value + 1), 0, (_pathBuffer.Length - 1));
                if (_manager.GetComponentData<RoadNodeComponent>(_pathBuffer[pathNodeIndex.value]).isOpen)
                    currentNode.node = _pathBuffer[pathNodeIndex.value];
                else
                    pathNodeIndex.value--;

                return;
            }

            //set movement
            #region movement
            {
                //set steering
                var _mapDirection = (_nodeMapPos - _aiMapPos);
                var _worldDirection = new float3(_mapDirection.x, 0, _mapDirection.y);
                //debug
                DrawRay(_aiPosition, _worldDirection, UnityEngine.Color.green);

                var _rotation = quaternion.LookRotation(_worldDirection, _aiUp);
                _rotation = math.mul(_rotation, quaternion.RotateY(-90));

                //debug
                DrawRay(_aiPosition, _aiUp, UnityEngine.Color.green);
                DrawRay(_aiPosition, math.forward(_rotation), UnityEngine.Color.blue);
                DrawRay(_aiPosition, math.cross(_aiUp, math.forward(_rotation)), UnityEngine.Color.red);

                steering.currentRotation = _rotation;

                //set acceleration
                float _directionLength = math.length(_worldDirection);
                float _forwardValue = math.dot(_worldDirection, _aiForwrd);
                int _acceleration = (int)(_forwardValue / _directionLength * 100);
                engine.acceleration = _acceleration;
            }
            #endregion
        });
    }
}