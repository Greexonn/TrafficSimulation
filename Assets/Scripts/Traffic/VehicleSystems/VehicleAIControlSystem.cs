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
            ref VehicleSteeringComponent steering, ref VehicleEngineComponent engine, ref VehicleBrakesComponent brakes) =>
        {
            var _aiTransforms = _manager.GetComponentData<LocalToWorld>(aiComponent.vehicleAITransform);
            var _aiPosition = _aiTransforms.Position;
            var _aiUp = _aiTransforms.Up;
            var _aiForwrd = _aiTransforms.Forward;

            var _pathBuffer = _manager.GetBuffer<NodeBufferElement>(vehicleEntity).Reinterpret<Entity>();

            //get nodes data
            pathNodeIndex.value = math.clamp(pathNodeIndex.value, 0, (_pathBuffer.Length - 1));
            int _nextNodeId = math.clamp((pathNodeIndex.value + 1), 0, (_pathBuffer.Length - 1));
            int _thirdNodeId = math.clamp((pathNodeIndex.value + 2), 0, (_pathBuffer.Length - 1));
            var _currentNodePos = _manager.GetComponentData<LocalToWorld>(currentNode.node).Position;
            var _nextNodePos = _manager.GetComponentData<LocalToWorld>(_pathBuffer[_nextNodeId]).Position;
            var _thirdNodePos = _manager.GetComponentData<LocalToWorld>(_pathBuffer[_thirdNodeId]).Position;

            //check if we've reached our target
            if (currentNode.node.Equals(_pathBuffer[_pathBuffer.Length - 1]))
            {
                _manager.AddComponent(vehicleEntity, typeof(PathfindingRequestComponent));
                return;
            }

            #region next node reaching

            //check if we've reached current target node
            //vehicle pos on the map
            float _mapX = math.dot(_aiPosition, _mapRight);
            float _mapY = math.dot(_aiPosition, _mapForward);
            var _aiMapPos = new float2(_mapX, _mapY);
            //next node pos on the map
            _mapX = math.dot(_nextNodePos, _mapRight);
            _mapY = math.dot(_nextNodePos, _mapForward);
            var _nextNodeMapPos = new float2(_mapX, _mapY);
            //current node pos on the map
            _mapX = math.dot(_currentNodePos, _mapRight);
            _mapY = math.dot(_currentNodePos, _mapForward);
            var _currentNodeMapPos = new float2(_mapX, _mapY);
            //vector from current to next node on the map
            var _currentToNext = _nextNodeMapPos - _currentNodeMapPos;
            //direction from current to next node on the map
            var _currentToNextDirection = math.normalize(_currentToNext);
            //vector from vehicle to next node on the map
            var _vehicleToNext = _aiMapPos - _currentNodeMapPos;
            //current path part projection
            float _pathPartProjection = math.dot(_currentToNext, _currentToNextDirection);
            //current from node to vehicle vector projection
            float _nodeToVehicleProjection = math.dot(_vehicleToNext, _currentToNextDirection);

            //get next node if reached
            if (_nodeToVehicleProjection >= _pathPartProjection)
            {
                pathNodeIndex.value = _nextNodeId;
                if (_manager.GetComponentData<RoadNodeComponent>(_pathBuffer[pathNodeIndex.value]).isOpen)
                {
                    currentNode.node = _pathBuffer[pathNodeIndex.value];
                }
                else
                {
                    pathNodeIndex.value--;

                    engine.acceleration = 0;
                    brakes.brakesUsage = 100;
                }

                return;
            }

            #endregion

            #region calculate next turn angle

            //third node map pos
            _mapX = math.dot(_thirdNodePos, _mapRight);
            _mapY = math.dot(_thirdNodePos, _mapForward);
            var _thirdNodeMapPos = new float2(_mapX, _mapY);

            //angle parts
            var _firstPart = _nextNodeMapPos - _currentNodeMapPos;
            var _secontPart = _thirdNodeMapPos - _nextNodeMapPos;
            //find lengths
            var _firstLength = math.length(_firstPart);
            _firstLength = math.dot(_secontPart, _firstPart) / _firstLength;
            var _secondLength = math.length(_secontPart);
            //find cos
            float _angleCos = _firstLength / _secondLength;
            //find angle
            float _angle = math.acos(_angleCos);

            //calculate turn angle koef
            float _turnAngleKoef = 1.0f - (_angle / math.PI);
            if (float.IsNaN(_turnAngleKoef))
                _turnAngleKoef = 0.5f;

            #endregion

            //set movement
            #region movement
            {
                //set steering
                var _direction = _nextNodeMapPos - _aiMapPos;
                var _worldDirection = new float3(_direction.x, 0, _direction.y);
                //debug
                DrawRay(_aiPosition, _worldDirection, UnityEngine.Color.blue);
                DrawLine((_aiPosition + _worldDirection), new float3(_thirdNodeMapPos.x, _aiPosition.y, _thirdNodeMapPos.y), UnityEngine.Color.red);

                _worldDirection = math.normalize(_worldDirection);
                var _rotation = quaternion.LookRotation(_worldDirection, _aiUp);

                //debug
                //DrawRay(_aiPosition, _aiUp, UnityEngine.Color.green);
                //DrawRay(_aiPosition, math.forward(_rotation), UnityEngine.Color.blue);
                //DrawRay(_aiPosition, math.cross(_aiUp, math.forward(_rotation)), UnityEngine.Color.red);

                steering.currentRotation = _rotation;

                //set acceleration
                float _directionLength = math.length(_worldDirection);
                float _forwardValue = math.dot(_worldDirection, _aiForwrd);
                int _acceleration = (int)(_forwardValue / _directionLength * _turnAngleKoef * 100);
                engine.acceleration = _acceleration;

                //set brakes
                if (!_manager.GetComponentData<RoadNodeComponent>(_pathBuffer[_nextNodeId]).isOpen)//if in front of closed node
                {
                    float _depth = 1.0f - (_nodeToVehicleProjection / _pathPartProjection);
                    if (_depth > 0.9f)
                    {
                        brakes.brakesUsage = (int)(_depth * 100);
                        engine.acceleration = 0;
                    }
                    else
                    {
                        float _koef = (2.0f / engine.currentSpeed);
                        brakes.brakesUsage = (int)((1.0f - _koef) * 100);
                        engine.acceleration = (int)(_koef * 100);
                    }
                }
                else//set brakes in based on next turn
                {
                    brakes.brakesUsage = (int)(100 * (1.0f - _turnAngleKoef));
                    brakes.brakesUsage = math.clamp(brakes.brakesUsage, 1, 3);
                }
            }
            #endregion
        });
    }
}