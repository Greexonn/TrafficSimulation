using TrafficSimulation.Traffic.VehicleComponents;
using TrafficSimulation.Traffic.VehicleComponents.Wheel;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using static Unity.Entities.SystemAPI;

namespace TrafficSimulation.Traffic.Bakers.VehicleComponentsBakers
{
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial struct VehicleBakingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate(new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<VehicleTag, WheelElement, BrakeWheelElement, DriveWheelElement, ControlWheelElement>()
                .Build(ref state));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = new EntityCommandBuffer(state.WorldUpdateAllocator);

            var job = new BakeVehiclesJob
            {
                WheelElementLookup = GetBufferLookup<WheelElement>(true),
                BrakeWheelElementLookup = GetBufferLookup<BrakeWheelElement>(true),
                DriveWheelElementLookup = GetBufferLookup<DriveWheelElement>(true),
                ControlWheelElementLookup = GetBufferLookup<ControlWheelElement>(true),
                CommandBuffer = commandBuffer.AsParallelWriter()
            };
            job.ScheduleParallelByRef(state.Dependency).Complete();
            
            commandBuffer.Playback(state.EntityManager);
        }
        
        [BurstCompile]
        [WithAll(typeof(WheelElement), typeof(BrakeWheelElement), typeof(DriveWheelElement), typeof(ControlWheelElement))]
        private partial struct BakeVehiclesJob : IJobEntity
        {
            [ReadOnly] public BufferLookup<WheelElement> WheelElementLookup;
            [ReadOnly] public BufferLookup<BrakeWheelElement> BrakeWheelElementLookup;
            [ReadOnly] public BufferLookup<DriveWheelElement> DriveWheelElementLookup;
            [ReadOnly] public BufferLookup<ControlWheelElement> ControlWheelElementLookup;

            public EntityCommandBuffer.ParallelWriter CommandBuffer;
            [NativeSetThreadIndex] private int _threadIndex;

            private void Execute(Entity vehicleEntity)
            {
                // configure wheels
                var wheelsBuffer = WheelElementLookup[vehicleEntity].Reinterpret<Entity>().AsNativeArray();
                
                var vehicleRefData = new VehicleRefData
                {
                    Entity = vehicleEntity,
                    WheelsCount = wheelsBuffer.Length
                };
                
                foreach (var wheelEntity in wheelsBuffer)
                {
                    CommandBuffer.AddComponent(_threadIndex, wheelEntity, vehicleRefData);
                }
                
                // configure brake wheels
                var brakeWheelsBuffer = BrakeWheelElementLookup[vehicleEntity].Reinterpret<int>().AsNativeArray();
                foreach (var wheelId in brakeWheelsBuffer)
                {
                    CommandBuffer.AddComponent<BrakeWheelTag>(_threadIndex, wheelsBuffer[wheelId]);
                }
                
                // configure drive wheels
                var driveWheelsBuffer = DriveWheelElementLookup[vehicleEntity].Reinterpret<int>().AsNativeArray();
                foreach (var wheelId in driveWheelsBuffer)
                {
                    CommandBuffer.AddComponent<DriveWheelTag>(_threadIndex, wheelsBuffer[wheelId]);
                }
                
                // configure control wheels
                var controlWheelsBuffer = ControlWheelElementLookup[vehicleEntity].Reinterpret<int>().AsNativeArray();
                foreach (var wheelId in controlWheelsBuffer)
                {
                    CommandBuffer.AddComponent<ControlWheelTag>(_threadIndex, wheelsBuffer[wheelId]);
                }
            }
        }
    }
}