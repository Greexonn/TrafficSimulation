using TrafficSimulation.Traffic.VehicleComponents.DriveVehicle;
using Unity.Entities;
using UnityEngine.InputSystem;

namespace TrafficSimulation.Traffic.Systems.VehicleSystems.Input
{
    [UpdateBefore(typeof(SpeedCheckSystem))]
    public partial class VehicleInputSystem : SystemBase
    {
        private VehicleInputActions _inputActions;

        //input values
        private int _driveDirection;
        private int _steeringDirection;
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
            var driveDirection = _driveDirection;
            var steeringDirection = _steeringDirection;
            var brakesValue = _brakesValue;
            
            Entities
                .WithAll<VehiclePlayerControlComponent>()
                .ForEach((ref VehicleEngineData engine, ref VehicleSteeringData steering, ref VehicleBrakesData brakes) =>
            {
                //acceleration
                engine.Acceleration = 100 * driveDirection;
                //steering
                steering.Direction = steeringDirection;
                //brakes
                brakes.BrakesUsage = brakesValue;
            }).Run();
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
}