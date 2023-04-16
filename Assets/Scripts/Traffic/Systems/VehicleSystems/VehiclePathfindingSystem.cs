using System;
using TrafficSimulation.Traffic.RoadComponents;
using TrafficSimulation.Traffic.VehicleComponents;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;
using Random = Unity.Mathematics.Random;

namespace TrafficSimulation.Traffic.Systems.VehicleSystems
{
    [UpdateInGroup(typeof(PreprocessAISystemGroup))]
    public partial struct VehiclePathfindingSystem : ISystem
    {
        private EntityQuery _targetPointsQuery;
        private NativeArray<RoadTargetData> _targetPoints;

        private Random _random;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _targetPointsQuery = new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<RoadTargetData>()
                .Build(ref state);
            state.RequireForUpdate(_targetPointsQuery);
            state.RequireForUpdate<TrafficSystemData>();
            
            _random = Random.CreateFromIndex(17105);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _targetPoints = _targetPointsQuery.ToComponentDataArray<RoadTargetData>(state.WorldUpdateAllocator);
            if (_targetPoints.Length < 1)
            {
                _targetPoints.Dispose();
                return;
            }
            
            var trafficSystemData = GetSingleton<TrafficSystemData>();
            var commandBuffer = new EntityCommandBuffer(state.WorldUpdateAllocator);
            var jobs = new UnsafeList<PathfindingJob>(4, state.WorldUpdateAllocator);
            var jobHandles = new NativeList<JobHandle>(state.WorldUpdateAllocator);

            foreach (var (vehicleCurrentNodeDataRef, vehiclePathNodeIndexDataRef, vehicleEntity) 
                     in Query<RefRW<VehicleCurrentNodeData>, RefRW<VehiclePathNodeIndexData>>().WithEntityAccess().WithAll<VehicleTag, PathfindingRequest>())
            {
                ref var vehicleCurrentNode = ref vehicleCurrentNodeDataRef.ValueRW;
                ref var vehiclePathNodeIndex = ref vehiclePathNodeIndexDataRef.ValueRW;
                vehiclePathNodeIndex.Value = 0;
                
                //select target
                var targetId = _random.NextInt(0, _targetPoints.Length);
                var targetPoint = _targetPoints[targetId].Node;
                if (targetPoint.Equals(vehicleCurrentNode.Node))
                    return;
                
                var pathfindingJob = new PathfindingJob
                {
                    TargetVehicle = vehicleEntity,
                    FoundNode = vehicleCurrentNode.Node,
                    TargetPoint = targetPoint,
                    LocalToWorldComponents = GetComponentLookup<LocalToWorld>(true),
                    Graph = trafficSystemData.Graphs[0],
                    CloseList = new NativeList<PathNode>(state.WorldUpdateAllocator),
                    OpenList = new NativeList<PathNode>(state.WorldUpdateAllocator),
                    ReversePathList = new NativeList<NodeBufferElement>(state.WorldUpdateAllocator)
                };
                jobs.Add(pathfindingJob);
                jobHandles.Add(pathfindingJob.Schedule(state.Dependency));
                
                commandBuffer.RemoveComponent<PathfindingRequest>(vehicleEntity);
            }
            
            JobHandle.CompleteAll(jobHandles.AsArray());
            
            //write correct path to path buffer
            foreach (var pathfindingJob in jobs)
            {
                var pathBuffer = GetBuffer<NodeBufferElement>(pathfindingJob.TargetVehicle);
                pathBuffer.Clear();
                var reversePathList = pathfindingJob.ReversePathList;
                
                for (var i = reversePathList.Length - 1; i >= 0 ; i--)
                {
                    pathBuffer.Add(reversePathList[i]);
                }
            }
            
            commandBuffer.Playback(state.EntityManager);
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
                    NodeEntity = FoundNode,
                    ParentNode = Entity.Null,
                    SValue = 0,
                    RValue = distanceToFinish
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
                        if (OpenList[i].RValue >= bestNode.RValue)
                            continue;

                        bestNode = OpenList[i];
                        bestNodeId = i;
                    }

                    var bestNodePos = LocalToWorldComponents[bestNode.NodeEntity].Position;

                    //add all paths from "best" node
                    var variants = Graph.GetValuesForKey(bestNode.NodeEntity);
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
                        var sValue = bestNode.SValue + (int)(math.distance(bestNodePos, nodePos) * 10);
                        var fValue = (int)(math.distance(finishPos, nodePos) * 10);
                        var rValue = sValue + fValue;
                        var newNode = new PathNode
                        {
                            NodeEntity = node,
                            ParentNode = bestNode.NodeEntity,
                            SValue = sValue,
                            RValue = rValue
                        };

                        var nodeId = OpenList.IndexOf(newNode);

                        if (nodeId != -1)
                        {
                            if (rValue < OpenList[nodeId].RValue)
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
                ReversePathList.Add(new NodeBufferElement { Node = TargetPoint });
                ReversePathList.Add(new NodeBufferElement { Node = CloseList[^1].NodeEntity });
                var parentEntity = CloseList[^1].ParentNode;
                for (var i = CloseList.Length - 2; i >= 0; i--)
                {
                    var node = CloseList[i];
                    if (!node.Equals(parentEntity))
                        continue;

                    parentEntity = node.ParentNode;
                    ReversePathList.Add(new NodeBufferElement { Node = node.NodeEntity });
                }
            }
        }

        private struct PathNode : IEquatable<PathNode>, IEquatable<Entity>
        {
            public Entity NodeEntity;
            public Entity ParentNode;
            public int SValue, RValue;

            public bool Equals(PathNode other)
            {
                return NodeEntity.Equals(other.NodeEntity);
            }

            public bool Equals(Entity other)
            {
                return NodeEntity.Equals(other);
            }
        }
    }
}