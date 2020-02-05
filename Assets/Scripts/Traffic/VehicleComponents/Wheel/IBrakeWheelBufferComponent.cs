using Unity.Entities;

[InternalBufferCapacity(32)]
public struct IBrakeWheelBufferComponent : IBufferElementData
{
    public int wheelID;
}
