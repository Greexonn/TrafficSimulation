using TrafficSimulation.Traffic.VehicleComponents.Wheel;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Bakers.VehicleComponentsBakers
{
    public class WheelBaker : Baker<WheelAuthoring>
    {
        public override void Bake(WheelAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            var wheelData = authoring.wheel;
            var wheelModel = GetComponent<SuspensionAuthoring>().wheelModel;
            wheelData.wheelModel = GetEntity(wheelModel, TransformUsageFlags.Dynamic);
            wheelData.wheelPosition = wheelModel.position;
            
            AddComponent(entity, wheelData);
            AddComponent<WheelRaycastData>(entity);
        }
    }
}