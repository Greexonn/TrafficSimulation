using TrafficSimulation.Traffic.RoadComponents;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Bakers.RoadComponentsBakers
{
    public class CarSpawnerBaker : Baker<CarSpawnerAuthoring>
    {
        public override void Bake(CarSpawnerAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent<CarSpawnerComponent>(entity);
        }
    }
}