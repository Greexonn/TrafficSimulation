using System.Collections.Generic;
using Traffic.RoadComponents;
using Traffic.VehicleComponents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Traffic.VehicleSystems
{
    [UpdateInGroup(typeof(PreprocessAISystemGroup))]
    public partial class VehiclePathfindingSystem : SystemBase
    {
        private EntityQuery _targetPointsQuery;
        private NativeArray<RoadTargetData> _targetPoints;

        private List<INativeDisposable> _temporalContainers;
        private List<PathfindingJob> _jobs;
        
        protected override void OnCreate()
        {
            _targetPointsQuery = GetEntityQuery(typeof(RoadTargetData));
            RequireForUpdate(_targetPointsQuery);
            
            _temporalContainers = new List<INativeDisposable>();
            _jobs = new List<PathfindingJob>();
        }

        protected override void OnUpdate()
        {
            _targetPoints = _targetPointsQuery.ToComponentDataArray<RoadTargetData>(Allocator.Temp);
            if (_targetPoints.Length < 1)
            {
                _targetPoints.Dispose();
                return;
            }

            var nodeBuffers = GetBufferLookup<NodeBufferElement>();

            var localToWorldComponents = GetComponentLookup<LocalToWorld>(true);
            
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
            var jobHandles = new NativeList<JobHandle>(Allocator.Temp);
            _temporalContainers.Clear();
            _jobs.Clear();

            Entities
                .WithoutBurst()
                .WithAll<VehicleTag, PathfindingRequest>()
                .ForEach((Entity vehicleEntity, ref VehicleCurrentNodeData vehicleCurrentNode, ref VehiclePathNodeIndexData vehiclePathNodeIndex) => 
            {
                vehiclePathNodeIndex.value = 0;

                //select target
                var targetId = UnityEngine.Random.Range(0, _targetPoints.Length);
                var targetPoint = _targetPoints[targetId].Node;
                if (targetPoint.Equals(vehicleCurrentNode.node))
                    return;
                
                var pathfindingJob = new PathfindingJob
                {
                    TargetVehicle = vehicleEntity,
                    FoundNode = vehicleCurrentNode.node,
                    TargetPoint = targetPoint,
                    LocalToWorldComponents = localToWorldComponents,
                    Graph = TrafficSystem.Instance.Graphs[0],
                    CloseList = new NativeList<PathNode>(Allocator.TempJob),
                    OpenList = new NativeList<PathNode>(Allocator.TempJob),
                    ReversePathList = new NativeList<NodeBufferElement>(Allocator.TempJob),
                };
                
                _temporalContainers.Add(pathfindingJob.CloseList);
                _temporalContainers.Add(pathfindingJob.OpenList);
                _temporalContainers.Add(pathfindingJob.ReversePathList);
                
                _jobs.Add(pathfindingJob);
                
                //remove request component
                commandBuffer.RemoveComponent<PathfindingRequest>(vehicleEntity);
            }).Run();
            
            // schedule
            foreach (var job in _jobs)
            {
                jobHandles.Add(job.Schedule(Dependency));
            }
            
            JobHandle.CompleteAll(jobHandles.AsArray());

            //write correct path to path buffer
            foreach (var pathfindingJob in _jobs)
            {
                var pathBuffer = nodeBuffers[pathfindingJob.TargetVehicle];
                pathBuffer.Clear();
                var reversePathList = pathfindingJob.ReversePathList;
                
                for (var i = reversePathList.Length - 1; i >= 0 ; i--)
                {
                    pathBuffer.Add(reversePathList[i]);
                }
            }

            commandBuffer.Playback(EntityManager);

            jobHandles.Dispose();
            commandBuffer.Dispose();
            _targetPoints.Dispose();

            foreach (var temporalContainer in _temporalContainers)
            {
                temporalContainer.Dispose();
            }
        }
        
        [BurstCompile]
        private struct PathfindingJob : IJob
        {
            public Entity TargetVehicle;
            
            public Entity TargetPoint;
            public Entity FoundNode;

            [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldComponents;

            [ReadOnly] public NativeParallelMultiHashMap<Entity, Entity> Graph;

            public NativeList<PathNode> CloseList;
            public NativeList<PathNode> OpenList;
            public NativeList<NodeBufferElement> ReversePathList;

            public void Execute()
            {
                //perform pathfinding
                var startPos = LocalToWorldComponents[FoundNode].Position;
                var finishPos = LocalToWorldComponents[TargetPoint].Position;
                var distanceToFinish = (int)(math.distance(finishPos, startPos) * 10);
                OpenList.Add(new PathNode
                {
                    nodeEntity = FoundNode,
                    parentNode = Entity.Null,
                    sValue = 0,
                    fValue = distanceToFinish,
                    rValue = distanceToFinish
                });
                
                while (!FoundNode.Equals(TargetPoint))
                {
                    if (OpenList.Length < 1)
                    {
                        UnityEngine.Debug.Log("Not Found");
                        break;
                    }
                    //get node with min result value
                    var bestNode = OpenList[0];
                    var bestNodeId = 0;
                    for (var i = 1; i < OpenList.Length; i++)
                    {
                        if (OpenList[i].rValue >= bestNode.rValue) 
                            continue;
                        
                        bestNode = OpenList[i];
                        bestNodeId = i;
                    }

                    var bestNodePos = LocalToWorldComponents[bestNode.nodeEntity].Position;

                    //add all paths from "best" node
                    var variants = Graph.GetValuesForKey(bestNode.nodeEntity);
                    foreach (var node in variants)
                    {
                        if (node.Equals(TargetPoint))
                        {
                            FoundNode = node;
                            break;
                        }

                        if (CloseList.Contains(node)) 
                            continue;
                        
                        var nodePos = LocalToWorldComponents[node].Position;
                        var sValue = bestNode.sValue + (int)(math.distance(bestNodePos, nodePos) * 10);
                        var fValue = (int) (math.distance(finishPos, nodePos) * 10);
                        var rValue = sValue + fValue;
                        var newNode = new PathNode
                        {
                            nodeEntity = node,
                            parentNode = bestNode.nodeEntity,
                            sValue = sValue,
                            fValue = fValue,
                            rValue = rValue
                        };
                        
                        var nodeId = OpenList.IndexOf(newNode);

                        if (nodeId != -1)
                        {
                            if (rValue < OpenList[nodeId].rValue)
                            {
                                OpenList[nodeId] = newNode;
                            }
                        }
                        else
                        {
                            OpenList.Add(newNode);
                        }
                    }

                    OpenList.RemoveAtSwapBack(bestNodeId);
                    CloseList.Add(bestNode);
                }
                
                //get correct reverse path
                ReversePathList.Add(new NodeBufferElement{node = TargetPoint});
                ReversePathList.Add(new NodeBufferElement{node = CloseList[^1].nodeEntity});
                var parentEntity = CloseList[^1].parentNode;
                for (var i = CloseList.Length - 2; i >= 0; i--)
                {
                    var node = CloseList[i];
                    if (!node.Equals(parentEntity)) 
                        continue;
                    
                    parentEntity = node.parentNode;
                    ReversePathList.Add(new NodeBufferElement{node = node.nodeEntity});
                }
            }
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
}