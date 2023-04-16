using Unity.Entities;
using UnityEngine;

namespace TrafficSimulation.Traffic.VehicleComponents
{
    public class VehicleAICollisionDetectionAuthoring : MonoBehaviour
    {
        public GameObject leftRayPoint, rightRayPoint;
    }
    
    public struct VehicleAICollisionDetectionComponent : IComponentData
    {
        public Entity LeftRayPoint, RightRayPoint;
    }
}
