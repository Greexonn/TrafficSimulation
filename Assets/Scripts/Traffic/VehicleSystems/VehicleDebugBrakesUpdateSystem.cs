using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[UpdateBefore(typeof(VehicleSuspensionSystem))]
public class VehicleDebugBrakesUpdateSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        return Entities.ForEach((ref VehicleBrakesComponent brakes) =>
        {
            brakes.brakesUsage = 10;
        }).Schedule(inputDependencies);
    }
}