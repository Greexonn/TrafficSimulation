using Unity.Entities;

namespace TrafficSimulation.Traffic.VehicleComponents.Wheel
{
    [InternalBufferCapacity(32)]
    public struct ControlWheelElement : IBufferElementData
    {
        public int WheelID;
    }
}
