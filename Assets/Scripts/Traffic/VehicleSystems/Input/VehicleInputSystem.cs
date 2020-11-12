﻿using Traffic.VehicleComponents.DriveVehicle;
using Traffic.VehicleSystems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.InputSystem;
using static UnityEngine.Debug;

[UpdateBefore(typeof(SpeedCheckSystem))]
public class VehicleInputSystem : ComponentSystem
{
    private VehicleInputActions _inputActions;

    //input values
    private int _driveDirection = 0;
    private int _steeringDirection = 0;
    private int _brakesValue = 1;

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
        //brakes
        _inputActions.Default.Brakes.started += StartBrakes;
        _inputActions.Default.Brakes.canceled += StopBrakes;
    }

    protected override void OnDestroy()
    {
        _inputActions.Dispose();
    }

    protected override void OnUpdate()
    {
        Entities.WithAll(typeof(VehiclePlayerControlComponent)).ForEach((ref VehicleEngineData engine, ref VehicleSteeringData steering, ref VehicleBrakesData brakes) =>
        {
            //acceleration
            engine.acceleration = 100 * _driveDirection;
            //steering
            steering.direction = _steeringDirection;
            //brakes
            brakes.brakesUsage = _brakesValue;
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

    #region brakes

    private void StartBrakes(InputAction.CallbackContext context)
    {
        _brakesValue = 100;
    }

    private void StopBrakes(InputAction.CallbackContext context)
    {
        _brakesValue = 1;
    }

    #endregion
}