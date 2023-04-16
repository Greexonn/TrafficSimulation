using TrafficSimulation.Traffic.RoadComponents;
using TrafficSimulation.Traffic.VehicleComponents;
using TrafficSimulation.Traffic.VehicleComponents.DriveVehicle;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;
using static UnityEngine.Debug;

namespace TrafficSimulation.Traffic.Systems.VehicleSystems
{
    [UpdateInGroup(typeof(ProcessAISystemGroup))]
    public partial struct VehicleAIControlSystem : ISystem
    {
        private float3 _mapForward;
        private float3 _mapRight;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _mapForward = new float3(0, 0, 1);
            _mapRight = new float3(1, 0, 0);
            
            state.RequireForUpdate(new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<VehicleTag, VehicleCurrentNodeData, VehiclePathNodeIndexData, VehicleSteeringData, VehicleEngineData, VehicleBrakesData, VehicleAIData>()
                .WithNone<PathfindingRequest>()
                .Build(ref state));
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            
            var job = new VehicleAIControlJob
            {
                LocalToWorldLookup = GetComponentLookup<LocalToWorld>(true),
                RoadNodeDataLookup = GetComponentLookup<RoadNodeData>(true),
                NodeBufferLookup = GetBufferLookup<NodeBufferElement>(true),
                MapRight = _mapRight,
                MapForward = _mapForward,
                CommandBuffer = commandBuffer
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(VehicleTag))]
        [WithNone(typeof(PathfindingRequest))]
        private partial struct VehicleAIControlJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;
            [ReadOnly] public ComponentLookup<RoadNodeData> RoadNodeDataLookup;
            [ReadOnly] public BufferLookup<NodeBufferElement> NodeBufferLookup;

            public float3 MapRight, MapForward;

            public EntityCommandBuffer.ParallelWriter CommandBuffer;

            [NativeSetThreadIndex] private int _nativeThreadIndex;
            
            private void Execute(Entity vehicleEntity, ref VehicleCurrentNodeData currentNode, ref VehiclePathNodeIndexData pathNodeIndex,
                ref VehicleSteeringData steering, ref VehicleEngineData engine, ref VehicleBrakesData brakes, in VehicleAIData aiData)
            {
                var aiTransforms = LocalToWorldLookup[aiData.VehicleAITransform];
                var aiPosition = aiTransforms.Position;
                var aiUp = aiTransforms.Up;
                var aiForward = aiTransforms.Forward;

                var pathBuffer = NodeBufferLookup[vehicleEntity].Reinterpret<Entity>();
                
                //get nodes data
                pathNodeIndex.Value = math.clamp(pathNodeIndex.Value, 0, pathBuffer.Length - 1);
                var nextNodeId = math.clamp(pathNodeIndex.Value + 1, 0, pathBuffer.Length - 1);
                var thirdNodeId = math.clamp(pathNodeIndex.Value + 2, 0, pathBuffer.Length - 1);
                var currentNodePos = LocalToWorldLookup[currentNode.Node].Position;
                var nextNodePos = LocalToWorldLookup[pathBuffer[nextNodeId]].Position;
                var thirdNodePos = LocalToWorldLookup[pathBuffer[thirdNodeId]].Position;
                
                //check if we've reached our target
                if (currentNode.Node.Equals(pathBuffer[^1]))
                {
                    CommandBuffer.AddComponent<PathfindingRequest>(_nativeThreadIndex, vehicleEntity);
                    return;
                }
                
                #region next node reaching

                    //check if we've reached current target node
                    //vehicle pos on the map
                    var mapX = math.dot(aiPosition, MapRight);
                    var mapY = math.dot(aiPosition, MapForward);
                    var aiMapPos = new float2(mapX, mapY);
                    //next node pos on the map
                    mapX = math.dot(nextNodePos, MapRight);
                    mapY = math.dot(nextNodePos, MapForward);
                    var nextNodeMapPos = new float2(mapX, mapY);
                    //current node pos on the map
                    mapX = math.dot(currentNodePos, MapRight);
                    mapY = math.dot(currentNodePos, MapForward);
                    var currentNodeMapPos = new float2(mapX, mapY);
                    //vector from current to next node on the map
                    var currentToNext = nextNodeMapPos - currentNodeMapPos;
                    //direction from current to next node on the map
                    var currentToNextDirection = math.normalize(currentToNext);
                    //vector from vehicle to next node on the map
                    var vehicleToNext = aiMapPos - currentNodeMapPos;
                    //current path part projection
                    var pathPartProjection = math.dot(currentToNext, currentToNextDirection);
                    //current from node to vehicle vector projection
                    var nodeToVehicleProjection = math.dot(vehicleToNext, currentToNextDirection);

                    //get next node if reached
                    if (nodeToVehicleProjection >= pathPartProjection)
                    {
                        pathNodeIndex.Value = nextNodeId;
                        var pathNodeData = RoadNodeDataLookup[pathBuffer[pathNodeIndex.Value]];
                        if (pathNodeData.IsOpen)
                        {
                            currentNode.Node = pathBuffer[pathNodeIndex.Value];
                        }
                        else
                        {
                            pathNodeIndex.Value--;

                            engine.Acceleration = 0;
                            brakes.BrakesUsage = 100;
                        }

                        return;
                    }

                    #endregion
                    
                    #region calculate next turn angle

                    //third node map pos
                    mapX = math.dot(thirdNodePos, MapRight);
                    mapY = math.dot(thirdNodePos, MapForward);
                    var thirdNodeMapPos = new float2(mapX, mapY);

                    //angle parts
                    var firstPart = nextNodeMapPos - currentNodeMapPos;
                    var secondPart = thirdNodeMapPos - nextNodeMapPos;
                    //find lengths
                    var firstLength = math.length(firstPart);
                    firstLength = math.dot(secondPart, firstPart) / firstLength;
                    var secondLength = math.length(secondPart);
                    //find cos
                    var angleCos = firstLength / secondLength;
                    //find angle
                    var angle = math.acos(angleCos);

                    //calculate turn angle koef
                    var turnAngleKoef = 1.0f - angle / math.PI;
                    if (float.IsNaN(turnAngleKoef))
                        turnAngleKoef = 0.5f;

                    #endregion
                    
                    //set movement
                    #region movement
                    {
                        //set steering
                        float2 direction;
                        if (turnAngleKoef < 0.9f) //smooth path
                        {
                            //four points
                            var p0 = currentNodeMapPos;
                            var p1 = aiMapPos;
                            var p2 = nextNodeMapPos;
                            var p3 = thirdNodeMapPos;

                            //four T's
                            const float t0 = 0.0f;
                            var t1 = GetT(t0, p0, p1);
                            var t2 = GetT(t1, p1, p2);
                            var t3 = GetT(t2, p2, p3);

                            var t = math.lerp(t1, t2, 0.1f);
                            var targetPoint = CalmullRom(t0, t1, t2, t3, p0, p1, p2, p3, t);
                            direction = targetPoint - aiMapPos;

                            if (float.IsNaN(direction.x) || float.IsNaN(direction.y))
                                direction = nextNodeMapPos - aiMapPos;
                        }
                        else
                        {
                            direction = nextNodeMapPos - aiMapPos;
                        }

                        var worldDirection = new float3(direction.x, 0, direction.y);
                        //debug
                        DrawRay(aiPosition, worldDirection, UnityEngine.Color.blue);
                        DrawLine(aiPosition + worldDirection, new float3(thirdNodeMapPos.x, aiPosition.y, thirdNodeMapPos.y), UnityEngine.Color.red);

                        worldDirection = math.normalize(worldDirection);
                        var rotation = quaternion.LookRotation(worldDirection, aiUp);

                        //debug
                        DrawRay(aiPosition, aiUp, UnityEngine.Color.green);
                        DrawRay(aiPosition, math.forward(rotation), UnityEngine.Color.blue);
                        DrawRay(aiPosition, math.cross(aiUp, math.forward(rotation)), UnityEngine.Color.red);

                        steering.CurrentRotation = math.mul(math.inverse(aiTransforms.Rotation), rotation);

                        //set acceleration
                        var directionLength = math.length(worldDirection);
                        var forwardValue = math.dot(worldDirection, aiForward);
                        var acceleration = (int)(forwardValue / directionLength * turnAngleKoef * 100);
                        engine.Acceleration = acceleration;

                        //set brakes
                        var nextNodeData = RoadNodeDataLookup[pathBuffer[nextNodeId]];
                        if (!nextNodeData.IsOpen) //if in front of closed node
                        {
                            var depth = 1.0f - nodeToVehicleProjection / pathPartProjection;
                            if (depth > 0.9f)
                            {
                                brakes.BrakesUsage = (int)(depth * 100);
                                engine.Acceleration = 0;
                            }
                            else
                            {
                                var coeff = 2.0f / engine.CurrentSpeed;
                                brakes.BrakesUsage = (int)((1.0f - coeff) * 100);
                                engine.Acceleration = (int)(coeff * 100);
                            }
                        }
                        else //set brakes in based on next turn
                        {
                            var recommendedSpeed = engine.MaxSpeed * turnAngleKoef;
                            var coeff = recommendedSpeed / engine.CurrentSpeed;
                            brakes.BrakesUsage = (int)(100 * (1.0f - coeff));
                            brakes.BrakesUsage = math.clamp(brakes.BrakesUsage, 1, 3);
                        }
                    }
                    #endregion
            }
        }

        private static float2 CalmullRom(float t0, float t1, float t2, float t3, float2 p0, float2 p1, float2 p2, float2 p3, float t)
        {
            var a1 = (t1 - t) / (t1 - t0) * p0 + (t - t0) / (t1 - t0) * p1;
            var a2 = (t2 - t) / (t2 - t1) * p1 + (t - t1) / (t2 - t1) * p2;
            var a3 = (t3 - t) / (t3 - t2) * p2 + (t - t2) / (t3 - t2) * p3;

            var b1 = (t2 - t) / (t2 - t0) * a1 + (t - t0) / (t2 - t0) * a2;
            var b2 = (t3 - t) / (t3 - t1) * a2 + (t - t1) / (t3 - t1) * a3;

            var c = (t2 - t) / (t2 - t1) * b1 + (t - t1) / (t2 - t1) * b2;

            return c;
        }

        private static float GetT(float t, float2 p0, float2 p1)
        {
            var a = math.pow(p1.x - p0.x, 2) + math.pow(p1.y - p0.y, 2);
            var b = math.pow(a, 0.5f);
            var c = math.pow(b, 0.5f);

            return c + t;
        }
    }
}