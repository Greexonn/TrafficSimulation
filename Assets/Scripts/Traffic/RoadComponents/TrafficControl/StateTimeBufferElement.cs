using Unity.Entities;

[InternalBufferCapacity(10)]
public struct StateTimeBufferElement : IBufferElementData
{
    public int value;
}
