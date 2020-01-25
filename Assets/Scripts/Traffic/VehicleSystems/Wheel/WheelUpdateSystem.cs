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
public class WheelUpdateSystem : ComponentSystem
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

        Entities.ForEach((Entity wheelEntity, ref WheelComponent wheel, ref SuspensionComponent suspension, ref LocalToWorld localToWorld) =>
        {
            int _vehicleRBIndex = _physicsWorld.GetRigidBodyIndex(wheel.vehiclePhysicsBody);
            if (_vehicleRBIndex == -1 || _vehicleRBIndex >= _physicsWorld.NumDynamicBodies)
                return;
            
            var _suspensionCenter = localToWorld.Position;
            var _wheelModelTransforms = _manager.GetComponentData<LocalToWorld>(wheel.wheelModel);
            var _currentWheelPos = _wheelModelTransforms.Position;
            var _dirUp = localToWorld.Up;
            var _dirForward = localToWorld.Forward;
            var _dirRight = localToWorld.Right;

            //cast ray
            CollisionFilter _filter = _physicsWorld.GetCollisionFilter(_vehicleRBIndex);
            RaycastInput _raycastInput = new RaycastInput
            {
                Start = (_suspensionCenter + (_dirUp * suspension.suspensionLength / 2)),
                End = (_suspensionCenter - (_dirUp * (wheel.radius + suspension.suspensionLength / 2))),
                Filter = _filter
            };
            RaycastHit _hit;

            if (_physicsWorld.CastRay(_raycastInput, out _hit))
            {
                var _vehiclePos = _manager.GetComponentData<LocalToWorld>(wheel.vehiclePhysicsBody).Position;
                var _vehicleCenterOfMass = _physicsWorld.GetCenterOfMass(_vehicleRBIndex);

                //calculate velocity at wheel
                var _wheelPos = _hit.Position - (_vehiclePos - _vehicleCenterOfMass);
                var _velocityAtWheel = _physicsWorld.GetLinearVelocity(_vehicleRBIndex, _wheelPos);

                //calculate slip factor
                float _slopeSlipFactor = math.pow(math.abs(math.dot(_dirUp, math.up())), 4.0f);

                //apply volocity changes
                float _currentSpeedUp = math.dot(_velocityAtWheel, _dirUp);
                float _currentSpeedForward = math.dot(_velocityAtWheel, _dirForward);
                float _currentSpeedRight = math.dot(_velocityAtWheel, _dirRight);

                float _fraction = _hit.Fraction - (wheel.radius / (suspension.suspensionLength + wheel.radius));
                float3 _wheelDesiredPos = math.lerp(_raycastInput.Start, (_raycastInput.End + wheel.radius), _fraction);
                var _pos = math.lerp(_currentWheelPos, _wheelDesiredPos, suspension.damperStrength / suspension.springStrength);
                _pos = _pos - _suspensionCenter;//get local position
                _manager.SetComponentData<Translation>(wheel.wheelModel, new Translation { Value = _pos });

                #region suspension
                {
                    //impulses
                    var _posA = _raycastInput.End;
                    var _posB = _hit.Position;
                    var _lvA = _currentSpeedUp * _dirUp;
                    var _lvB = _physicsWorld.GetLinearVelocity(_hit.RigidBodyIndex, _posB);

                    var _impulse = suspension.springStrength * (_posB - _posA) + suspension.damperStrength * (_lvB - _lvA);
                    // TO-DO inv wheel count
                    float _impulseUp = math.dot(_impulse, _dirUp);

                    float _downForceLimit = -0.25f;
                    if (_downForceLimit < _impulseUp)
                    {
                        _impulse = _impulseUp * _dirUp;
                        _impulse *= _physicsWorld.GetEffectiveMass(_vehicleRBIndex, _dirUp, _suspensionCenter) * 0.6f;

                        _physicsWorld.ApplyImpulse(_vehicleRBIndex, _impulse, _suspensionCenter);

                        //debug
                        Debug.DrawRay(_suspensionCenter, _impulse, Color.red);
                    }
                }
                #endregion

                //debug
                Debug.DrawLine(_raycastInput.Start, _hit.Position, Color.green);
            }
            else
            {
                var _wheelDesiredPos = _suspensionCenter - (_dirUp * suspension.suspensionLength / 2);
                var _pos = math.lerp(_currentWheelPos, _wheelDesiredPos, suspension.damperStrength / suspension.springStrength);
                _pos = _pos - _suspensionCenter;//get local position
                _manager.SetComponentData<Translation>(wheel.wheelModel, new Translation { Value = _pos });

                //debug
                //Debug.Log("desired: " + _wheelDesiredPos + " current: " + _wheelModelTransforms.Position);
                Debug.DrawLine(_currentWheelPos, _wheelDesiredPos, Color.green);
            }
        });
    }
}