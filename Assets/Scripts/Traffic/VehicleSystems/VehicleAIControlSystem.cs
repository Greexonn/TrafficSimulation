using Traffic.VehicleComponents;
using Traffic.VehicleComponents.DriveVehicle;
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

        Entities.WithAll(typeof(VehicleTag)).WithNone(typeof(PathfindingRequestComponent)).
            ForEach((Entity vehicleEntity, ref VehicleAIComponent aiComponent, ref VehicleCurrentNodeComponent currentNode, ref VehiclePathNodeIndexComponent pathNodeIndex,
            ref VehicleSteeringData steering, ref VehicleEngineData engine, ref VehicleBrakesData brakes) =>
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

            #region smooth path

            ////four points
            //var _p0 = _currentNodeMapPos;
            //var _p1 = _aiMapPos;
            //var _p2 = _nextNodeMapPos;
            //var _p3 = _thirdNodeMapPos;

            ////four T's
            //float _t0 = 0.0f;
            //float _t1 = GetT(_t0, _p0, _p1);
            //float _t2 = GetT(_t1, _p1, _p2);
            //float _t3 = GetT(_t2, _p2, _p3);

            ////draw
            //float2 _lastPoint = _p0;
            //for (float i = _t1; i < _t3; i += _deltaTime)
            //{
            //    float2 _C = CalmullRom(_t0, _t1, _t2, _t3, _p0, _p1, _p2, _p3, i);

            //    float3 _startPoint = new float3(_lastPoint.x, _aiPosition.y, _lastPoint.y);
            //    float3 _endPoint = new float3(_C.x, _aiPosition.y, _C.y);
            //    _lastPoint = _C;
            //    UnityEngine.Color _color = UnityEngine.Color.blue;
            //    if (i > _t2)
            //        _color = UnityEngine.Color.red;

            //    DrawLine(_startPoint, _endPoint, _color);
            //}

            #endregion

            //set movement
            #region movement
            {
                //set steering
                var _direction = float2.zero;
                if (_turnAngleKoef < 0.9f)//smooth path
                {
                    //four points
                    var _p0 = _currentNodeMapPos;
                    var _p1 = _aiMapPos;
                    var _p2 = _nextNodeMapPos;
                    var _p3 = _thirdNodeMapPos;

                    //four T's
                    float _t0 = 0.0f;
                    float _t1 = GetT(_t0, _p0, _p1);
                    float _t2 = GetT(_t1, _p1, _p2);
                    float _t3 = GetT(_t2, _p2, _p3);

                    float _t = math.lerp(_t1, _t2, 0.1f);
                    var _targetPoint = CalmullRom(_t0, _t1, _t2, _t3, _p0, _p1, _p2, _p3, _t);
                    _direction = _targetPoint - _aiMapPos;

                    if (float.IsNaN(_direction.x) || float.IsNaN(_direction.y))
                        _direction = _nextNodeMapPos - _aiMapPos;
                }
                else
                {
                    _direction = _nextNodeMapPos - _aiMapPos;
                }

                var _worldDirection = new float3(_direction.x, 0, _direction.y);
                //debug
                //DrawRay(_aiPosition, _worldDirection, UnityEngine.Color.blue);
                //DrawLine((_aiPosition + _worldDirection), new float3(_thirdNodeMapPos.x, _aiPosition.y, _thirdNodeMapPos.y), UnityEngine.Color.red);

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
                    float _recomendedSpeed = engine.maxSpeed * _turnAngleKoef;
                    float _koef = (_recomendedSpeed / engine.currentSpeed);
                    brakes.brakesUsage = (int)(100 * (1.0f - _koef));
                    brakes.brakesUsage = math.clamp(brakes.brakesUsage, 1, 3);
                }
            }
            #endregion
        });
    }

    private float2 CalmullRom(float t0, float t1, float t2, float t3, float2 p0, float2 p1, float2 p2, float2 p3, float t)
    {
        float2 _A1 = (t1 - t) / (t1 - t0) * p0 + (t - t0) / (t1 - t0) * p1;
        float2 _A2 = (t2 - t) / (t2 - t1) * p1 + (t - t1) / (t2 - t1) * p2;
        float2 _A3 = (t3 - t) / (t3 - t2) * p2 + (t - t2) / (t3 - t2) * p3;

        float2 _B1 = (t2 - t) / (t2 - t0) * _A1 + (t - t0) / (t2 - t0) * _A2;
        float2 _B2 = (t3 - t) / (t3 - t1) * _A2 + (t - t1) / (t3 - t1) * _A3;

        float2 _C = (t2 - t) / (t2 - t1) * _B1 + (t - t1) / (t2 - t1) * _B2;

        return _C;
    }

    private float GetT(float t, float2 p0, float2 p1)
    {
        float _a = math.pow((p1.x - p0.x), 2) + math.pow((p1.y - p0.y), 2);
        float _b = math.pow(_a, 0.5f);
        float _c = math.pow(_b, 0.5f);

        return (_c + t);
    }
}