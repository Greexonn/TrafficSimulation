using TrafficSimulation.Traffic.VehicleComponents;
using TrafficSimulation.Traffic.VehicleComponents.Wheel;
using Unity.Entities;

namespace TrafficSimulation.Traffic.Bakers.VehicleComponentsBakers
{
    public class VehicleBaker : Baker<VehicleAuthoring>
    {
        public override void Bake(VehicleAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent<VehicleTag>(entity);
            
            //add wheel buffers
            var wheelBuffer = AddBuffer<WheelElement>(entity);
            var driveWheelBuffer = AddBuffer<DriveWheelElement>(entity);
            var controlWheelBuffer = AddBuffer<ControlWheelElement>(entity);
            var breakWheelBuffer = AddBuffer<BrakeWheelElement>(entity);
            
            foreach (var wheel in authoring._wheels)
            {
                var wheelEntity = GetEntity(wheel, TransformUsageFlags.Dynamic);
                wheelBuffer.Add(new WheelElement { Wheel = wheelEntity });
            }
            
            foreach (var wheel in authoring._driveWheels)
            {
                driveWheelBuffer.Add(new DriveWheelElement { WheelID = authoring._wheels.IndexOf(wheel) });
            }
            
            foreach (var wheel in authoring._controlWheels)
            {
                controlWheelBuffer.Add(new ControlWheelElement { WheelID = authoring._wheels.IndexOf(wheel) });
            }
            
            foreach (var wheel in authoring._breakWheels)
            {
                breakWheelBuffer.Add(new BrakeWheelElement { WheelID = authoring._wheels.IndexOf(wheel) });
            }
        }
    }
}