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
    private int _acceleration;

    protected override void OnCreate()
    {
        _inputActions = new VehicleInputActions();
        _inputActions.Enable();
        //subscribe
        _inputActions.Default.Acceleration.performed += UpdateAcceleration;
        _inputActions.Default.Acceleration.canceled += StopAcceleration;
    }

    protected override void OnDestroy()
    {
        _inputActions.Dispose();
    }

    protected override void OnUpdate()
    {
        Entities.WithAll(typeof(VehiclePlayerControlComponent)).ForEach((ref VehicleEngineComponent engine) =>
        {
            engine.acceleration = 100;
            engine.direction = _acceleration;
        });
    }

    private void UpdateAcceleration(InputAction.CallbackContext context)
    {
        _acceleration = (int)context.ReadValue<float>();
    }

    private void StopAcceleration(InputAction.CallbackContext context)
    {
        _acceleration = 0;
    }
}