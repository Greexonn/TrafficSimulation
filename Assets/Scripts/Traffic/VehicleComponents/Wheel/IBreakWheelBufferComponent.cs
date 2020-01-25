using Unity.Entities;

[InternalBufferCapacity(32)]
public struct IBreakWheelBufferComponent : IBufferElementData
{
    public Entity wheel;
}
