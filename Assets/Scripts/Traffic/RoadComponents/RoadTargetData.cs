using Unity.Entities;

namespace Traffic.RoadComponents
{
    [GenerateAuthoringComponent]
    public struct RoadTargetData : IComponentData
    {
        public Entity node;
    }
}
