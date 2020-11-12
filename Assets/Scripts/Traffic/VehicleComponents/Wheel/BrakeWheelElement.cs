using Unity.Entities;

namespace Traffic.VehicleComponents.Wheel
{
    [InternalBufferCapacity(32)]
    public struct BrakeWheelElement : IBufferElementData
    {
        public int wheelID;
    }
}
