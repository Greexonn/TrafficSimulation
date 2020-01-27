using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Physics.Extensions;

[UpdateAfter(typeof(BuildPhysicsWorld))]
public class VehicleDebugEngineUpdateSystem : ComponentSystem
{
    private BuildPhysicsWorld _buildPhysicsWorldSystem;

    protected override void OnCreate()
    {
        _buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        _buildPhysicsWorldSystem.FinalJobHandle.Complete();

        PhysicsWorld _physicsWorld = _buildPhysicsWorldSystem.PhysicsWorld;

        Entities.ForEach((Entity vehicleEntity, ref VehicleEngineComponent engine, ref LocalToWorld transforms) =>
        {
            int _vehicleRBIndex = _physicsWorld.GetRigidBodyIndex(vehicleEntity);
            if (_vehicleRBIndex == -1 || _vehicleRBIndex >= _physicsWorld.NumDynamicBodies)
                return;

            var _dirForward = transforms.Right;

            var _vehicleVelocity = _physicsWorld.GetLinearVelocity(_vehicleRBIndex);
            float _currentSpeedForward = math.dot(_vehicleVelocity, _dirForward);

            engine.currentSpeed = math.abs(_currentSpeedForward);

            //debug
            engine.direction = 1;
            engine.acceleration = 50;

        });
    }
}