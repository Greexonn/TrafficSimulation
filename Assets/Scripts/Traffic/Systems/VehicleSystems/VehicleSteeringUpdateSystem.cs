using TrafficSimulation.Traffic.VehicleComponents;
using TrafficSimulation.Traffic.VehicleComponents.DriveVehicle;
using Unity.Entities;
using Unity.Mathematics;

namespace TrafficSimulation.Traffic.Systems.VehicleSystems
{
    [UpdateBefore(typeof(SpeedCheckSystem))]
    public partial class VehicleSteeringUpdateSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            Dependency = Entities
                .WithNone<VehicleAIData>()
                .ForEach((ref VehicleSteeringData steering) =>
                {
                    steering.CurrentTransition += steering.Direction * steering.SteeringSpeed * deltaTime;
                    if (steering.Direction == 0)
                        steering.CurrentTransition = 0.5f;
                    steering.CurrentTransition = math.clamp(steering.CurrentTransition, 0, 1);

                    var leftBoundRotation = quaternion.EulerXYZ(0, steering.MaxAngle, 0);
                    var rightBoundRotation = quaternion.EulerXYZ(0, -steering.MaxAngle, 0);

                    steering.CurrentRotation = math.nlerp(leftBoundRotation, rightBoundRotation, steering.CurrentTransition);
                }).Schedule(Dependency);
        }
    }
}