using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.InputSystem;
using static UnityEngine.Debug;

[UpdateBefore(typeof(VehicleSuspensionSystem))]
public class VehicleInputSystem : ComponentSystem
{
    private VehicleInputActions _inputActions;

    //input values
    private int _driveDirection = 0;
    private int _steeringDirection = 0;

    protected override void OnCreate()
    {
        _inputActions = new VehicleInputActions();
        _inputActions.Enable();
        //subscribe
        //acceleration
        _inputActions.Default.Acceleration.performed += UpdateAcceleration;
        _inputActions.Default.Acceleration.canceled += StopAcceleration;
        //steering
        _inputActions.Default.Steering.performed += UpdateSteering;
        _inputActions.Default.Steering.canceled += StopSteering;
    }

    protected override void OnDestroy()
    {
        _inputActions.Dispose();
    }

    protected override void OnUpdate()
    {
        Entities.WithAll(typeof(VehiclePlayerControlComponent)).ForEach((ref VehicleEngineComponent engine, ref VehicleSteeringComponent steering) =>
        {
            //acceleration
            engine.acceleration = 100;
            engine.direction = _driveDirection;
            //steering
            steering.direction = _steeringDirection;
        });
    }

    #region acceleration
    private void UpdateAcceleration(InputAction.CallbackContext context)
    {
        _driveDirection = (int)context.ReadValue<float>();
    }

    private void StopAcceleration(InputAction.CallbackContext context)
    {
        _driveDirection = 0;
    }

    #endregion

    #region steering

    private void UpdateSteering(InputAction.CallbackContext context)
    {
        _steeringDirection = (int)context.ReadValue<float>();
    }

    private void StopSteering(InputAction.CallbackContext context)
    {
        _steeringDirection = 0;
    }

    #endregion
}