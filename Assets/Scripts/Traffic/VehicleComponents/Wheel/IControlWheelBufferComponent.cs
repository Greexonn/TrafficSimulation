using Unity.Entities;

[InternalBufferCapacity(32)]
public struct IControlWheelBufferComponent : IBufferElementData
{
    public int wheelID;
}
