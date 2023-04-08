using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace TrafficSimulation.Traffic.VehicleComponents.Wheel
{
    [DisallowMultipleComponent]
    public class WheelAuthoring : MonoBehaviour
    {
        [FormerlySerializedAs("wheel")] 
        [SerializeField] public WheelData _wheel;

        public void Convert(Entity entity, EntityManager dstManager)
        {
            var wheelModel = GetComponent<SuspensionAuthoring>().wheelModel;
            // _wheel.wheelModel = conversionSystem.GetPrimaryEntity(wheelModel);
            _wheel.wheelPosition = wheelModel.position;

            dstManager.AddComponentData(entity, _wheel);
            dstManager.AddComponent<WheelRaycastData>(entity);

#if UNITY_EDITOR
            dstManager.SetName(entity, "Wheel");
#endif
        }
    }
}
