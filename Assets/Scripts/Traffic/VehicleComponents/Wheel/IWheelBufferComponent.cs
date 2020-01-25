using Unity.Entities;

[InternalBufferCapacity(32)]
public struct IWheelBufferComponent : IBufferElementData
{
    public Entity wheel;
}
