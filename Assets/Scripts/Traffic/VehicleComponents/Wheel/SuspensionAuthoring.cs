using UnityEngine;
using UnityEngine.Serialization;

namespace TrafficSimulation.Traffic.VehicleComponents.Wheel
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(WheelAuthoring))]
    public class SuspensionAuthoring : MonoBehaviour
    {
        [FormerlySerializedAs("_suspension")]
        [SerializeField] public SuspensionData suspension;
        [FormerlySerializedAs("_springStrengthKoef")]
        [SerializeField]
        public float springStrengthCoeff;
        [FormerlySerializedAs("_damperStrengthKoef")]
        [SerializeField]
        public float damperStrengthCoeff;
        [SerializeField] public Transform wheelModel;
        [FormerlySerializedAs("_wheelPos")]
        [SerializeField] [Range(0, 1)] private float wheelPos;
        [FormerlySerializedAs("_body")]
        [SerializeField] public Unity.Physics.Authoring.PhysicsBodyAuthoring body;

        private WheelAuthoring _wheel;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            //draw suspension line
            var transformCache = transform;
            var fromPos = transformCache.position;
            var toPos = fromPos - transformCache.up * suspension.suspensionLength;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(fromPos, toPos);
            //draw suspension ends
            var right = transformCache.right;
            var fromEnd = fromPos - right * suspension.suspensionLength / 10;
            var toEnd = fromPos + right * suspension.suspensionLength / 10;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(fromEnd, toEnd);
            fromEnd = toPos - right * suspension.suspensionLength / 10;
            toEnd = toPos + right * suspension.suspensionLength / 10;
            Gizmos.DrawLine(fromEnd, toEnd);
            //place model in pos
            var wheelCenter = Vector3.Lerp(fromPos, toPos, wheelPos);
            Gizmos.DrawWireSphere(wheelCenter, suspension.suspensionLength / 20);
            if (wheelModel != null)
            {
                wheelModel.position = wheelCenter;
            }
            //draw wheel
            UnityEditor.Handles.color = Color.green;
            if (_wheel != null)
            {
                var radius = _wheel.wheel.radius;
                UnityEditor.Handles.DrawWireDisc(wheelCenter, transform.right, radius);
            }
            else
                _wheel = GetComponent<WheelAuthoring>();
        }
#endif
    }
}
