using Unity.Entities;

namespace TrafficSimulation.Traffic.VehicleComponents.Wheel
{
    [InternalBufferCapacity(32)]
    public struct WheelElement : IBufferElementData
    {
        public Entity Wheel;
    }
}
