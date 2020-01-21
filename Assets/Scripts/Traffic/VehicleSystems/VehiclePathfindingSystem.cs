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
            if (_targetPoint.Equals(vehicleCurrentNode.node))
                return;
            // UnityEngine.Debug.Log("TargetNode: " + _targetPoint.ToString());

            Entity _foundNode = vehicleCurrentNode.node;

            //perform pathfinding
            NativeList<PathNode> _closeList = new NativeList<PathNode>(Allocator.Temp);
            NativeList<PathNode> _openList = new NativeList<PathNode>(Allocator.Temp);
            NativeList<NodeBufferElement> _reversePathList = new NativeList<NodeBufferElement>(Allocator.Temp);
            var _startPos = _manager.GetComponentData<LocalToWorld>(_foundNode).Position;
            var _finishPos = _manager.GetComponentData<LocalToWorld>(_targetPoint).Position;
            int _distanceToFinish = (int)(math.distance(_finishPos, _startPos) * 10);
            _openList.Add(new PathNode
            {
                nodeEntity = _foundNode,
                parentNode = Entity.Null,
                sValue = 0,
                fValue = _distanceToFinish,
                rValue = _distanceToFinish
            });

            while (!_foundNode.Equals(_targetPoint))
            {
                if (_openList.Length < 1)
                {
                    UnityEngine.Debug.Log("Not Found");
                    string _path = "";
                    foreach (var node in _closeList)
                    {
                        _path += _manager.GetName(node.nodeEntity) + "\n";
                    }
                    UnityEngine.Debug.Log(_path);
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

                var _bestNodePos = _manager.GetComponentData<LocalToWorld>(_bestNode.nodeEntity).Position;

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
                        int _sValue = _bestNode.sValue + (int)(math.distance(_bestNodePos, _nodePos) * 10);
                        int _fValue = (int)(math.distance(_finishPos, _nodePos) * 10);
                        int _rValue = _sValue + _fValue;
                        var _newNode = new PathNode
                        {
                            nodeEntity = node,
                            parentNode = _bestNode.nodeEntity,
                            sValue = _sValue,
                            fValue = _fValue,
                            rValue = _rValue
                        };
                        
                        int _nodeId = _openList.IndexOf(_newNode);

                        if (_nodeId != -1)
                        {
                            if (_rValue < _openList[_nodeId].rValue)
                            {
                                _openList[_nodeId] = _newNode;

                            }
                        }
                        else
                        {
                            _openList.Add(_newNode);
                        }
                    }
                }

                _openList.RemoveAtSwapBack(_bestNodeId);
                _closeList.Add(_bestNode);
            }

            //get correct reverse path
            _reversePathList.Add(new NodeBufferElement{node = _targetPoint});
            _reversePathList.Add(new NodeBufferElement{node = _closeList[_closeList.Length - 1].nodeEntity});
            Entity _parentEntity = _closeList[_closeList.Length - 1].parentNode;
            for (int i = (_closeList.Length - 2); i >= 0; i--)
            {
                var _node = _closeList[i];
                if (_node.Equals(_parentEntity))
                {
                    _parentEntity = _node.parentNode;
                    _reversePathList.Add(new NodeBufferElement{node = _node.nodeEntity});
                }
            }

            //write correct path to path buffer
            for (int i = (_reversePathList.Length - 1); i >= 0 ; i--)
            {
                _pathBuffer.Add(_reversePathList[i]);
            }

            //dispose temporal containers
            _openList.Dispose();
            _closeList.Dispose();
            _reversePathList.Dispose();

            //remove request component
            _manager.RemoveComponent(vehicleEntity, typeof(PathfindingRequestComponent));
        });
    }

    private struct PathNode : System.IEquatable<PathNode>, System.IEquatable<Entity>
    {
        public Entity nodeEntity;
        public Entity parentNode;
        public int sValue, fValue, rValue;

        public bool Equals(PathNode other)
        {
            return nodeEntity.Equals(other.nodeEntity);
        }

        public bool Equals(Entity other)
        {
            return nodeEntity.Equals(other);
        }
    }
}