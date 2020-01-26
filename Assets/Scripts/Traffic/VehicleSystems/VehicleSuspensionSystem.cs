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

        Entities.WithAll(typeof(VehicleComponent)).ForEach((Entity vehicleEntity) =>
        {
            int _vehicleRBIndex = _physicsWorld.GetRigidBodyIndex(vehicleEntity);
            if (_vehicleRBIndex == -1 || _vehicleRBIndex >= _physicsWorld.NumDynamicBodies)
                return;

            var _wheelElementBuffer = _manager.GetBuffer<IWheelBufferComponent>(vehicleEntity);
            var _wheelBuffer = _wheelElementBuffer.Reinterpret<Entity>();
            var _wheelArray = _wheelBuffer.AsNativeArray();

            var _vehicleTransforms = _manager.GetComponentData<LocalToWorld>(vehicleEntity);
            var _dirUp = _vehicleTransforms.Up;
            var _dirForward = _vehicleTransforms.Right;
            var _dirRight = _vehicleTransforms.Forward;

            foreach (var wheel in _wheelArray)
            {
                var _wheelComponent = _manager.GetComponentData<WheelComponent>(wheel);
                var _suspensionComponent = _manager.GetComponentData<SuspensionComponent>(wheel);

                var _wheelRoot = _manager.GetComponentData<LocalToWorld>(wheel);
                var _suspensionTop = _wheelRoot.Position;
                var _wheelRight = _wheelRoot.Forward;
                var _wheelTranslation = _manager.GetComponentData<Translation>(_wheelComponent.wheelModel);

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

                    #region set wheel position
                    {
                        float _fraction = _hit.Fraction - (_wheelComponent.radius / (_suspensionComponent.suspensionLength + _wheelComponent.radius));
                        _wheelPos = math.lerp(_raycastInput.Start, (_raycastInput.End + _wheelComponent.radius), _fraction);
                        var _wheelLocalPos = _wheelPos - _suspensionTop;
                        _wheelTranslation.Value.y = _wheelLocalPos.y;
                        _manager.SetComponentData<Translation>(_wheelComponent.wheelModel, _wheelTranslation);
                        _wheelPos = _wheelTranslation.Value + _suspensionTop;
                    }
                    #endregion

                    float _delta = math.abs(math.length(_wheelPos - _wheelComponent.wheelPosition));

                    #region suspension
                    {
                        //get spring compression'
                        float _suspensionCurrentLength = math.length(_wheelPos - _suspensionTop);
                        float _compression = 1.0f - (_suspensionCurrentLength / _suspensionComponent.suspensionLength);

                        var _impulse = _dirUp * (_compression * _suspensionComponent.springStrength - (_suspensionComponent.damperStrength * _delta));

                        _physicsWorld.ApplyImpulse(_vehicleRBIndex, _impulse, _suspensionTop);
                        _physicsWorld.ApplyImpulse(_hit.RigidBodyIndex, -_impulse, _hit.Position);

                        //debug
                        Debug.DrawRay(_wheelPos, _impulse);
                    }
                    #endregion

                    #region sideways friction
                    {
                        var _velocityAtWheel = _physicsWorld.GetLinearVelocity(_vehicleRBIndex, _hit.Position);

                        float _currentSpeedRight = math.dot(_velocityAtWheel, _wheelRight);

                        float _deltaSpeedRight = -_currentSpeedRight;
                        _deltaSpeedRight *= _wheelComponent.sideFriction;

                        var _maxImpulse = _wheelRight * _wheelComponent.maxSideFriction;
                        var _impulse = _deltaSpeedRight * _wheelRight;
                        _impulse *= _physicsWorld.GetEffectiveMass(_vehicleRBIndex, _impulse, _hit.Position);
                        _impulse = math.clamp(_impulse, -_maxImpulse, _maxImpulse);

                        _physicsWorld.ApplyImpulse(_vehicleRBIndex, _impulse, _wheelPos);
                        _physicsWorld.ApplyImpulse(_vehicleRBIndex, -_impulse, _hit.Position);

                        //debug
                        Debug.DrawRay(_wheelPos, _impulse, Color.red);
                    }
                    #endregion

                    //set new wheel psosition
                    _wheelComponent.wheelPosition = _wheelPos;
                    _manager.SetComponentData<WheelComponent>(wheel, _wheelComponent);
                }
                else
                {
                    var _wheelLocalPos = -_dirUp * _suspensionComponent.suspensionLength;
                    _manager.SetComponentData<Translation>(_wheelComponent.wheelModel, new Translation { Value = _wheelLocalPos });
                }
            }

            //dispose temporal containers
        });
    }
}