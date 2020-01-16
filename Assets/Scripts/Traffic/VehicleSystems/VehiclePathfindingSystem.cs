using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class VehiclePathfindingSystem : ComponentSystem
{
    private EntityManager _manager;
    private NativeArray<RoadTargetComponent> _targetPoints;

    protected override void OnCreate()
    {
        _manager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    protected override void OnDestroy()
    {
        if (_targetPoints.IsCreated)
            _targetPoints.Dispose();
    }

    protected override void OnUpdate()
    {
        
        if(!_targetPoints.IsCreated)
        {
            var _query = GetEntityQuery(typeof(RoadTargetComponent));
            _targetPoints = _query.ToComponentDataArray<RoadTargetComponent>(Allocator.Persistent);
        }
        else if (_targetPoints.Length < 1)
            return;

        Entities.WithAll(typeof(VehicleComponent), typeof(PathfindingRequestComponent)).ForEach((Entity vehicleEntity, ref VehicleCurrentNodeComponent vehicleCurrentNode, ref VehiclePathNodeIndexComponent vehiclePathNodeIndex) => 
        {
            var _pathBuffer = _manager.GetBuffer<NodeBufferElement>(vehicleEntity);
            _pathBuffer.Clear();
            _pathBuffer.Add(new NodeBufferElement{node = vehicleCurrentNode.node});
            vehiclePathNodeIndex.value = 0;

            //select target
            int _targetId = UnityEngine.Random.Range(0, _targetPoints.Length);
            Entity _targetPoint = _targetPoints[_targetId].node;
            // UnityEngine.Debug.Log("TargetNode: " + _targetPoint.ToString());

            //find path
            Entity _foundNode = vehicleCurrentNode.node;
            //very straight algorithm
            // int _stopCounter = 0;
            // while (!_foundNode.Equals(_targetPoint))
            // {
            //     _stopCounter++;
            //     NativeMultiHashMapIterator<Entity> _iterator;
            //     if (TrafficSystem.instance.graphs[0].TryGetFirstValue(_foundNode, out _foundNode, out _iterator))
            //     {
            //         _pathBuffer.Add(new NodeBufferElement{node = _foundNode});
            //         // UnityEngine.Debug.Log(_foundNode);
            //     }

            //     if (_stopCounter >= 20)
            //     {
            //         UnityEngine.Debug.Log("Not Found");
            //         break;
            //     }
            // }

            //perform pathfinding
            NativeList<Entity> _closeList = new NativeList<Entity>(TrafficSystem.instance.graphs[0].Length, Allocator.Temp);
            NativeList<PathNode> _openList = new NativeList<PathNode>(TrafficSystem.instance.graphs[0].Length, Allocator.Temp);
            _foundNode = vehicleCurrentNode.node;
            var _startPos = _manager.GetComponentData<LocalToWorld>(_foundNode).Position;
            var _finishPos = _manager.GetComponentData<LocalToWorld>(_targetPoint).Position;
            int _distanceToFinish = (int)(math.distance(_finishPos, _startPos) * 10);
            _openList.Add(new PathNode
            {
                nodeEntity = _foundNode,
                sValue = 0,
                fValue = _distanceToFinish,
                rValue = _distanceToFinish
            });

            while (!_foundNode.Equals(_targetPoint))
            {
                if (_openList.Length < 1)
                {
                    UnityEngine.Debug.Log("Not Found");
                    break;
                }
                //get node with min result value
                var _bestNode = _openList[0];
                var _bestNodeId = 0;
                for (int i = 1; i < _openList.Length; i++)
                {
                    if (_openList[i].rValue < _bestNode.rValue)
                    {
                        _bestNode = _openList[i];
                        _bestNodeId = i;
                    }
                }
                //add all paths from "best" node
                var _variants = TrafficSystem.instance.graphs[0].GetValuesForKey(_bestNode.nodeEntity);
                foreach (var node in _variants)
                {
                    if (node.Equals(_targetPoint))
                    {
                        _foundNode = node;
                        break;
                    }

                    if (!_closeList.Contains(node))
                    {
                        var _nodePos = _manager.GetComponentData<LocalToWorld>(node).Position;
                        int _sValue = (int)(math.distance(_startPos, _nodePos) * 10);
                        int _fValue = (int)(math.distance(_finishPos, _nodePos) * 10);
                        _openList.Add(new PathNode
                        {
                            nodeEntity = node,
                            sValue = _sValue,
                            fValue = _fValue,
                            rValue = _sValue + _fValue
                        });
                    }
                }

                _openList.RemoveAtSwapBack(_bestNodeId);
                _closeList.Add(_bestNode.nodeEntity);
            }


            for (int i = 0; i < _closeList.Length; i++)
            {
                _pathBuffer.Add(new NodeBufferElement{node = _closeList[i]});
            }
            _pathBuffer.Add(new NodeBufferElement{node = _targetPoint});

            //dispose temporal containers
            _openList.Dispose();
            _closeList.Dispose();

            //remove request component
            _manager.RemoveComponent(vehicleEntity, typeof(PathfindingRequestComponent));
        });
    }

    private struct PathNode
    {
        public Entity nodeEntity;
        public int sValue, fValue, rValue;
    }
}