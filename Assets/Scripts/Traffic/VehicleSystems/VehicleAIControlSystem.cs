using Traffic.RoadComponents;
using Traffic.VehicleComponents;
using Traffic.VehicleComponents.DriveVehicle;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Traffic.VehicleSystems
{
    [UpdateInGroup(typeof(ProcessAISystemGroup))]
    public class VehicleAIControlSystem : SystemBase
    {
        private float3 _mapForward;
        private float3 _mapRight;

        private EndSimulationEntityCommandBufferSystem _commandBufferSystem;

        protected override void OnCreate()
        {
            _mapForward = new float3(0, 0, 1);
            _mapRight = new float3(1, 0, 0);

            _commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;

            var roadNodeComponents = GetComponentDataFromEntity<RoadNodeData>(true);
            var localToWorldComponents = GetComponentDataFromEntity<LocalToWorld>(true);

            var nodeBuffers = GetBufferFromEntity<NodeBufferElement>(true);

            var commandBuffer = _commandBufferSystem.CreateCommandBuffer().AsParallelWriter();

            var mapForward = _mapForward;
            var mapRight = _mapRight;

            Entities
                .WithReadOnly(roadNodeComponents)
                .WithReadOnly(localToWorldComponents)
                .WithReadOnly(nodeBuffers)
                .WithAll<VehicleTag>()
                .WithNone<PathfindingRequest>()
                .ForEach((int nativeThreadIndex, Entity vehicleEntity, ref VehicleCurrentNodeData currentNode, ref VehiclePathNodeIndexData pathNodeIndex,
                    ref VehicleSteeringData steering, ref VehicleEngineData engine, ref VehicleBrakesData brakes, 
                    in VehicleAIData aiData) =>
                {
                    var aiTransforms = localToWorldComponents[aiData.vehicleAITransform];
                    var aiPosition = aiTransforms.Position;
                    var aiUp = aiTransforms.Up;
                    var aiForward = aiTransforms.Forward;

                    var pathBuffer = nodeBuffers[vehicleEntity].Reinterpret<Entity>();

                    //get nodes data
                    pathNodeIndex.value = math.clamp(pathNodeIndex.value, 0, pathBuffer.Length - 1);
                    var nextNodeId = math.clamp(pathNodeIndex.value + 1, 0, pathBuffer.Length - 1);
                    var thirdNodeId = math.clamp(pathNodeIndex.value + 2, 0, pathBuffer.Length - 1);
                    var currentNodePos = localToWorldComponents[currentNode.node].Position;
                    var nextNodePos = localToWorldComponents[pathBuffer[nextNodeId]].Position;
                    var thirdNodePos = localToWorldComponents[pathBuffer[thirdNodeId]].Position;

                    //check if we've reached our target
                    if (currentNode.node.Equals(pathBuffer[pathBuffer.Length - 1]))
                    {
                        commandBuffer.AddComponent<PathfindingRequest>(nativeThreadIndex, vehicleEntity);
                        return;
                    }

                    #region next node reaching

                    //check if we've reached current target node
                    //vehicle pos on the map
                    var mapX = math.dot(aiPosition, mapRight);
                    var mapY = math.dot(aiPosition, mapForward);
                    var aiMapPos = new float2(mapX, mapY);
                    //next node pos on the map
                    mapX = math.dot(nextNodePos, mapRight);
                    mapY = math.dot(nextNodePos, mapForward);
                    var nextNodeMapPos = new float2(mapX, mapY);
                    //current node pos on the map
                    mapX = math.dot(currentNodePos, mapRight);
                    mapY = math.dot(currentNodePos, mapForward);
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
                        pathNodeIndex.value = nextNodeId;
                        var pathNodeData = roadNodeComponents[pathBuffer[pathNodeIndex.value]];
                        if (pathNodeData.isOpen)
                        {
                            currentNode.node = pathBuffer[pathNodeIndex.value];
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
                    mapX = math.dot(thirdNodePos, mapRight);
                    mapY = math.dot(thirdNodePos, mapForward);
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
                    var turnAngleKoef = 1.0f - (angle / math.PI);
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
                        //DrawRay(_aiPosition, _worldDirection, UnityEngine.Color.blue);
                        //DrawLine((_aiPosition + _worldDirection), new float3(_thirdNodeMapPos.x, _aiPosition.y, _thirdNodeMapPos.y), UnityEngine.Color.red);

                        worldDirection = math.normalize(worldDirection);
                        var rotation = quaternion.LookRotation(worldDirection, aiUp);

                        //debug
                        //DrawRay(_aiPosition, _aiUp, UnityEngine.Color.green);
                        //DrawRay(_aiPosition, math.forward(_rotation), UnityEngine.Color.blue);
                        //DrawRay(_aiPosition, math.cross(_aiUp, math.forward(_rotation)), UnityEngine.Color.red);

                        steering.currentRotation = rotation;

                        //set acceleration
                        var directionLength = math.length(worldDirection);
                        var forwardValue = math.dot(worldDirection, aiForward);
                        var acceleration = (int)(forwardValue / directionLength * turnAngleKoef * 100);
                        engine.acceleration = acceleration;

                        //set brakes
                        var nextNodeData = roadNodeComponents[pathBuffer[nextNodeId]];
                        if (!nextNodeData.isOpen) //if in front of closed node
                        {
                            var depth = 1.0f - nodeToVehicleProjection / pathPartProjection;
                            if (depth > 0.9f)
                            {
                                brakes.brakesUsage = (int)(depth * 100);
                                engine.acceleration = 0;
                            }
                            else
                            {
                                var koef = 2.0f / engine.currentSpeed;
                                brakes.brakesUsage = (int)((1.0f - koef) * 100);
                                engine.acceleration = (int)(koef * 100);
                            }
                        }
                        else //set brakes in based on next turn
                        {
                            var recommendedSpeed = engine.maxSpeed * turnAngleKoef;
                            var koef = recommendedSpeed / engine.currentSpeed;
                            brakes.brakesUsage = (int)(100 * (1.0f - koef));
                            brakes.brakesUsage = math.clamp(brakes.brakesUsage, 1, 3);
                        }
                    }
                    #endregion
                }).ScheduleParallel(Dependency).Complete();
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