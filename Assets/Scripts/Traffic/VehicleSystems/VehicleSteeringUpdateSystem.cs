using Traffic.VehicleComponents;
using Traffic.VehicleComponents.DriveVehicle;
using Traffic.VehicleSystems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(SpeedCheckSystem))]
public class VehicleSteeringUpdateSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float _deltaTime = Time.DeltaTime;

        return Entities.WithNone<VehicleAIData>().ForEach((ref VehicleSteeringData steering) =>
        {
            steering.currentTransition += steering.direction * steering.steeringSpeed * _deltaTime;
            if (steering.direction == 0)
                steering.currentTransition = 0.5f;
            steering.currentTransition = math.clamp(steering.currentTransition, 0, 1);

            var _leftBoundRotation = quaternion.EulerXYZ(0, steering.maxAngle, 0);
            var _rightBoundRotation = quaternion.EulerXYZ(0, -steering.maxAngle, 0);

            steering.currentRotation = math.nlerp(_leftBoundRotation, _rightBoundRotation, steering.currentTransition);

        }).Schedule(inputDependencies);
    }
}