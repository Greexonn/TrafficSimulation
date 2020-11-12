using Unity.Entities;

namespace Traffic.VehicleComponents.Wheel
{
    [InternalBufferCapacity(32)]
    public struct DriveWheelElement : IBufferElementData
    {
        public int wheelID;
    }
}
