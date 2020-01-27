using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(VehicleSuspensionSystem))]
public class VehicleDebugSteeringUpdateSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float _deltaTime = Time.DeltaTime;

        return Entities.ForEach((ref VehicleSteeringComponent steering) =>
        {
            //debug
            steering.direction = 1;

            steering.currentTransition += steering.steeringSpeed * _deltaTime;
            steering.currentTransition = math.clamp(steering.currentTransition, 0, 1);

            var _leftBoundRotation = quaternion.EulerXYZ(0, steering.maxAngle, 0);
            var _rightBoundRotation = quaternion.EulerXYZ(0, -steering.maxAngle, 0);

            steering.currentRotation = math.nlerp(_leftBoundRotation, _rightBoundRotation, steering.currentTransition);

        }).Schedule(inputDependencies);
    }
}