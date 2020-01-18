using Unity.Entities;

[InternalBufferCapacity(10)]
public struct IntBufferElement : IBufferElementData
{
    public int value;
}
