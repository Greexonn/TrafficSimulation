using TrafficSimulation.Traffic.VehicleComponents;
using TrafficSimulation.Traffic.VehicleComponents.DriveVehicle;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Entities.SystemAPI;

namespace TrafficSimulation.Traffic.Systems.VehicleSystems
{
    [UpdateBefore(typeof(SpeedCheckSystem))]
    public partial struct VehicleSteeringUpdateSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate(new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<VehicleSteeringData>()
                .WithNone<VehicleAIData>()
                .Build(ref state));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new VehicleSteeringUpdateJob
            {
                DeltaTime = Time.DeltaTime
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
        
        [BurstCompile]
        [WithNone(typeof(VehicleAIData))]
        private partial struct VehicleSteeringUpdateJob : IJobEntity
        {
            public float DeltaTime;
            
            private void Execute(ref VehicleSteeringData steering)
            {
                steering.CurrentTransition += steering.Direction * steering.SteeringSpeed * DeltaTime;
                if (steering.Direction == 0)
                    steering.CurrentTransition = 0.5f;
                steering.CurrentTransition = math.clamp(steering.CurrentTransition, 0, 1);

                var leftBoundRotation = quaternion.EulerXYZ(0, steering.MaxAngle, 0);
                var rightBoundRotation = quaternion.EulerXYZ(0, -steering.MaxAngle, 0);

                steering.CurrentRotation = math.nlerp(leftBoundRotation, rightBoundRotation, steering.CurrentTransition);
            }
        }
    }
}