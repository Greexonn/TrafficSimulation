using Unity.Entities;

namespace Traffic.VehicleComponents.Wheel
{
    [InternalBufferCapacity(32)]
    public struct ControlWheelElement : IBufferElementData
    {
        public int wheelID;
    }
}
