using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

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
            int _stopCounter = 0;
            while (!_foundNode.Equals(_targetPoint))
            {
                _stopCounter++;
                NativeMultiHashMapIterator<Entity> _iterator;
                if (TrafficSystem.instance.graphs[0].TryGetFirstValue(_foundNode, out _foundNode, out _iterator))
                {
                    _pathBuffer.Add(new NodeBufferElement{node = _foundNode});
                    // UnityEngine.Debug.Log(_foundNode);
                }

                if (_stopCounter >= 20)
                {
                    UnityEngine.Debug.Log("Not Found");
                    break;
                }
            }
            //remove request component
            _manager.RemoveComponent(vehicleEntity, typeof(PathfindingRequestComponent));
        });
    }
}