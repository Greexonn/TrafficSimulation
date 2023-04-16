using Unity.Entities;

namespace TrafficSimulation.Traffic.VehicleComponents.Wheel
{
    [InternalBufferCapacity(32)]
    public struct BrakeWheelElement : IBufferElementData
    {
        public int WheelID;
    }
}
