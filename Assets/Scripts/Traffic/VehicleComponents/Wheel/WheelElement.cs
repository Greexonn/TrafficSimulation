using Unity.Entities;

namespace Traffic.VehicleComponents.Wheel
{
    [InternalBufferCapacity(32)]
    public struct WheelElement : IBufferElementData
    {
        public Entity wheel;
    }
}
