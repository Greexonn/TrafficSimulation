using Unity.Entities;

namespace TrafficSimulation.Traffic.VehicleComponents.Wheel
{
    [InternalBufferCapacity(32)]
    public struct DriveWheelElement : IBufferElementData
    {
        public int WheelID;
    }
}
