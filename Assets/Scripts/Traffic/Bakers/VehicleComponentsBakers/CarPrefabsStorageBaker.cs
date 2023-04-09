using TrafficSimulation.Traffic.Behaviours;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Bakers.VehicleComponentsBakers
{
    public class CarPrefabsStorageBaker : Baker<CarPrefabsStorageAuthoring>
    {
        public override void Bake(CarPrefabsStorageAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            var buffer = AddBuffer<CarPrefabsStorageElement>(entity);

            foreach (var authoringCarPrefab in authoring._carPrefabs)
            {
                buffer.Add(new CarPrefabsStorageElement
                {
                    PrefabEntity = GetEntity(authoringCarPrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}