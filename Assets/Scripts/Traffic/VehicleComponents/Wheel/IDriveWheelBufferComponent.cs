using Unity.Entities;

[InternalBufferCapacity(32)]
public struct IDriveWheelBufferComponent : IBufferElementData
{
    public Entity wheel;
}
