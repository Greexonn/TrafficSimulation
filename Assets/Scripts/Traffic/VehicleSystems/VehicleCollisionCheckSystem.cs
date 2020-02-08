using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Physics.Extensions;
using RaycastHit = Unity.Physics.RaycastHit;
using static UnityEngine.Debug;

[UpdateBefore(typeof(VehicleSuspensionSystem)), UpdateAfter(typeof(BuildPhysicsWorld))]
public class VehicleCollisionCheckSystem : ComponentSystem
{
    private EntityManager _manager;
    private BuildPhysicsWorld _buildPhysicsWorldSystem;

    protected override void OnCreate()
    {
        _manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        _buildPhysicsWorldSystem.FinalJobHandle.Complete();

        PhysicsWorld _physicsWorld = _buildPhysicsWorldSystem.PhysicsWorld;

        Entities.WithAll<VehicleComponent>().ForEach((Entity vehicleEntity, ref VehicleAICollisionDetectionComponent collisionDetection, ref LocalToWorld transforms, 
            ref VehicleSteeringComponent steering, ref VehicleEngineComponent engine, ref VehicleBrakesComponent brakes) =>
        {
            int _vehicleRBIndex = _physicsWorld.GetRigidBodyIndex(vehicleEntity);
            if (_vehicleRBIndex == -1 || _vehicleRBIndex >= _physicsWorld.NumDynamicBodies)
                return;

            var _rayDirection = math.forward(steering.currentRotation);
            if (math.dot(_rayDirection, transforms.Forward) < 0)
                return;

            var _vehicleVelocity = _physicsWorld.GetLinearVelocity(_vehicleRBIndex);
            var _velocityInDirection = math.dot(_vehicleVelocity, _rayDirection);
            _velocityInDirection = math.clamp(_velocityInDirection, 1, _velocityInDirection);

            var _leftPos = _manager.GetComponentData<LocalToWorld>(collisionDetection.leftRayPoint).Position;
            var _rightPos = _manager.GetComponentData<LocalToWorld>(collisionDetection.rightRayPoint).Position;

            var _ray = _rayDirection * _velocityInDirection;

            //cast rays
            RaycastInput _raycastInputLeft = new RaycastInput
            {
                Start = _leftPos,
                End = (_leftPos + _ray),
                Filter = CollisionFilter.Default
            };
            RaycastHit _hitLeft;
            RaycastInput _raycastInputRigt = new RaycastInput
            {
                Start = _rightPos,
                End = (_rightPos + _ray),
                Filter = CollisionFilter.Default
            };
            RaycastHit _hitRight;

            bool _isLeftHit = _physicsWorld.CastRay(_raycastInputLeft, out _hitLeft);
            bool _isRightHit = _physicsWorld.CastRay(_raycastInputRigt, out _hitRight);

            float _fraction = 1;

            if (_isLeftHit && _isRightHit)
            {
                _fraction = _hitLeft.Fraction;
                if (_hitRight.Fraction < _fraction)
                    _fraction = _hitRight.Fraction;

                //debug
                DrawLine(_raycastInputLeft.Start, _hitLeft.Position, UnityEngine.Color.red);
                DrawLine(_raycastInputRigt.Start, _hitRight.Position, UnityEngine.Color.red);
            }
            else if (_isLeftHit)
            {
                _fraction = _hitLeft.Fraction;

                //debug
                DrawLine(_raycastInputLeft.Start, _hitLeft.Position, UnityEngine.Color.red);
            }
            else if (_isRightHit)
            {
                _fraction = _hitRight.Fraction;

                //debug
                DrawLine(_raycastInputRigt.Start, _hitRight.Position, UnityEngine.Color.red);
            }
            else
            {
                return;
            }

            //set brakes
            float _closeValue = 1.0f - _fraction;
            int _usage = (int)(_closeValue * 100);
            brakes.brakesUsage = _usage;
            engine.acceleration = 0;
        });
    }
}