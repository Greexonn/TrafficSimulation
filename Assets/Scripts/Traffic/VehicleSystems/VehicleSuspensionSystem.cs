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
using UnityEngine;

[UpdateAfter(typeof(BuildPhysicsWorld))]
public class VehicleSuspensionSystem : ComponentSystem
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

        Entities.WithAll(typeof(VehicleComponent)).ForEach((Entity vehicleEntity, ref VehicleEngineComponent engine, ref VehicleSteeringComponent steering, ref VehicleBrakesComponent brakes) =>
        {
            int _vehicleRBIndex = _physicsWorld.GetRigidBodyIndex(vehicleEntity);
            if (_vehicleRBIndex == -1 || _vehicleRBIndex >= _physicsWorld.NumDynamicBodies)
                return;

            //get wheels buffer
            var _wheelArray = _manager.GetBuffer<IWheelBufferComponent>(vehicleEntity).Reinterpret<Entity>().AsNativeArray();
            //get drive wheels ids
            var _driveIdsArray = _manager.GetBuffer<IDriveWheelBufferComponent>(vehicleEntity).Reinterpret<int>().AsNativeArray();
            //get control wheels ids
            var _controlIdsArray = _manager.GetBuffer<IControlWheelBufferComponent>(vehicleEntity).Reinterpret<int>().AsNativeArray();
            //get brake wheels ids
            var _brakeIdsArray = _manager.GetBuffer<IBrakeWheelBufferComponent>(vehicleEntity).Reinterpret<int>().AsNativeArray();

            var _vehicleTransforms = _manager.GetComponentData<LocalToWorld>(vehicleEntity);
            var _dirUp = _vehicleTransforms.Up;
            var _dirForward = _vehicleTransforms.Right;
            var _dirRight = _vehicleTransforms.Forward;

            ////debug linear velocity
            //if (math.length(_physicsWorld.GetLinearVelocity(_vehicleRBIndex)) < 0.1f)
            //{
            //    _physicsWorld.SetLinearVelocity(_vehicleRBIndex, float3.zero);
            //}

            for (int i = 0; i < _wheelArray.Length; i++)
            {
                var _wheel = _wheelArray[i];

                var _wheelComponent = _manager.GetComponentData<WheelComponent>(_wheel);
                var _suspensionComponent = _manager.GetComponentData<SuspensionComponent>(_wheel);

                var _wheelRoot = _manager.GetComponentData<LocalToWorld>(_wheel);
                var _suspensionTop = _wheelRoot.Position;
                var _wheelRight = _wheelRoot.Forward;
                var _wheelForward = _wheelRoot.Right;

                //cast ray
                CollisionFilter _filter = _physicsWorld.GetCollisionFilter(_vehicleRBIndex);
                RaycastInput _raycastInput = new RaycastInput
                {
                    Start = _suspensionTop,
                    End = (_suspensionTop - (_dirUp * (_wheelComponent.radius + _suspensionComponent.suspensionLength))),
                    Filter = _filter
                };
                RaycastHit _hit;

                if (_physicsWorld.CastRay(_raycastInput, out _hit))
                {
                    float3 _wheelPos;
                    var _velocityAtWheel = _physicsWorld.GetLinearVelocity(_vehicleRBIndex, _hit.Position);

                    //debug
                    //Debug.DrawLine(_raycastInput.Start, _raycastInput.End, Color.green);

                    #region set wheel position
                    {
                        _wheelPos = _hit.Position + _dirUp * _wheelComponent.radius;
                        var _wheelLocalPos = _wheelPos - _suspensionTop;
                        _manager.SetComponentData<Translation>(_wheelComponent.wheelModel, new Translation { Value = _wheelLocalPos });
                    }
                    #endregion

                    #region suspension
                    {
                        //get vehicle up speed
                        var _vehicleUpAtWheel = _physicsWorld.GetLinearVelocity(_vehicleRBIndex, _suspensionTop);
                        float _speedUp = math.dot(_vehicleUpAtWheel, _dirUp);
                        //get spring compression'
                        float _suspensionCurrentLength = math.length(_wheelPos - _suspensionTop);

                        float _compression = 1.0f - (_suspensionCurrentLength / _suspensionComponent.suspensionLength);

                        var _impulseValue = _compression * _suspensionComponent.springStrength - _suspensionComponent.damperStrength * _speedUp / 10;

                        if (_impulseValue > 0)
                        {
                            var _impulse = _dirUp * _impulseValue;

                            _physicsWorld.ApplyImpulse(_vehicleRBIndex, _impulse, _suspensionTop);
                            _physicsWorld.ApplyImpulse(_hit.RigidBodyIndex, -_impulse, _hit.Position);
                        }

                    }
                    #endregion

                    #region sideways friction
                    {
                        //debug
                        //Debug.DrawRay(_wheelPos, _velocityAtWheel, Color.green);

                        float _currentSpeedRight = math.dot(_velocityAtWheel, _wheelRight);

                        //debug
                        //Debug.DrawRay(_wheelPos, _wheelRight * _currentSpeedRight, Color.red);

                        float _impulseValue = -_currentSpeedRight * _wheelComponent.sideFriction;
                        var _impulse = _impulseValue * _wheelRight;

                        float _effectiveMass = _physicsWorld.GetEffectiveMass(_vehicleRBIndex, _impulse, _hit.Position) / _wheelArray.Length;
                        _impulseValue *= _effectiveMass;

                        _impulseValue = math.clamp(_impulseValue, -_wheelComponent.maxSideFriction, _wheelComponent.maxSideFriction);
                        _impulse = _impulseValue * _wheelRight;

                        //debug
                        //Debug.DrawRay(_wheelPos, _impulse, Color.blue);

                        _physicsWorld.ApplyImpulse(_vehicleRBIndex, _impulse, _hit.Position);
                        _physicsWorld.ApplyImpulse(_hit.RigidBodyIndex, -_impulse, _hit.Position);

                        //debug
                        //Debug.DrawRay(_wheelPos, _impulse, Color.red);
                    }
                    #endregion

                    #region breaks
                    {
                        //if current wheel is brake wheel
                        if (_brakeIdsArray.Contains(i))
                        {
                            var _velocityForward = math.dot(_velocityAtWheel, _wheelForward);

                            //debug
                            //Debug.DrawRay(_wheelPos, _wheelForward * _velocityForward, Color.white);

                            float _impulseValue = -_velocityForward * _wheelComponent.forwardFriction;
                            var _impulse = _wheelForward * _impulseValue;
                            float _effectiveMass = _physicsWorld.GetEffectiveMass(_vehicleRBIndex, _impulse, _hit.Position);
                            _impulseValue *= _effectiveMass * brakes.brakesUsage / 100;
                            _impulseValue = math.clamp(_impulseValue, -_wheelComponent.maxForwardFriction, _wheelComponent.maxForwardFriction);

                            _impulse = _wheelForward * _impulseValue;

                            _physicsWorld.ApplyImpulse(_vehicleRBIndex, _impulse, _hit.Position);
                            _physicsWorld.ApplyImpulse(_hit.RigidBodyIndex, -_impulse, _hit.Position);

                            //debug
                            //Debug.DrawRay(_wheelPos, _impulse, Color.red);
                        }
                    }
                    #endregion

                    #region drive
                    {
                        //if current wheel is drive wheel
                        if (_driveIdsArray.Contains(i))
                        {
                            float _direction = math.dot(_wheelForward, _dirForward);
                            _direction /= math.abs(_direction);

                            var _impulse = _wheelForward * _direction;
                            float _impulseKoef = 1.0f - (engine.currentSpeed / engine.maxSpeed);
                            float _effectiveMass = _physicsWorld.GetEffectiveMass(_vehicleRBIndex, _impulse, _wheelPos);
                            float _impulseValue = _effectiveMass * _wheelComponent.forwardFriction * _impulseKoef * engine.direction;
                            _impulseValue = math.clamp(_impulseValue, -_wheelComponent.maxForwardFriction, _wheelComponent.maxForwardFriction);

                            _impulse *= _impulseValue;

                            _physicsWorld.ApplyImpulse(_vehicleRBIndex, _impulse, _wheelPos);
                            _physicsWorld.ApplyImpulse(_hit.RigidBodyIndex, -_impulse, _hit.Position);

                            //debug
                            Debug.DrawRay(_wheelPos, _impulse, Color.red);
                        }
                    }
                    #endregion

                    //set new wheel psosition
                    _wheelComponent.wheelPosition = _wheelPos;
                    _manager.SetComponentData<WheelComponent>(_wheel, _wheelComponent);
                }
                else
                {
                    var _wheelDesiredPos = _suspensionTop - _dirUp * _suspensionComponent.suspensionLength;
                    float _height = math.dot((_wheelComponent.wheelPosition - _suspensionTop), (_wheelDesiredPos - _suspensionTop));
                    float _fraction = _suspensionComponent.suspensionLength / _height;
                    _fraction += (_suspensionComponent.damperStrength / _suspensionComponent.springStrength);
                    _fraction = math.clamp(_fraction, 0, 1);
                    var _wheelPos = math.lerp(_suspensionTop, _wheelDesiredPos, _fraction);
                    var _wheelLocalPos = _wheelPos - _suspensionTop;
                    _manager.SetComponentData<Translation>(_wheelComponent.wheelModel, new Translation { Value = _wheelLocalPos });

                    //set new wheel psosition
                    _wheelComponent.wheelPosition = _wheelPos;
                    _manager.SetComponentData<WheelComponent>(_wheel, _wheelComponent);

                }
            }

            //dispose temporal containers
        });
    }
}