using Unity.Entities;
using UnityEngine;

namespace Traffic.VehicleComponents.Wheel
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(WheelAuthoring))]
    public class SuspensionAuthoring : MonoBehaviour
    {
        [SerializeField] private SuspensionData _suspension;
        [SerializeField] private float _springStrengthKoef;
        [SerializeField] private float _damperStrengthKoef;
        [SerializeField] public Transform wheelModel;
        [SerializeField] [Range(0, 1)] private float _wheelPos;
        [SerializeField] Unity.Physics.Authoring.PhysicsBodyAuthoring _body;

        private WheelAuthoring _wheel;

        public void Convert(Entity entity, EntityManager dstManager)
        {
            var vehicleMass = _body.Mass;

            _suspension.springStrength = vehicleMass / 10 * _springStrengthKoef;
            _suspension.damperStrength = vehicleMass / 10 * _damperStrengthKoef;

            dstManager.AddComponentData(entity, _suspension);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            //draw suspension line
            var transformCache = transform;
            var fromPos = transformCache.position;
            var toPos = fromPos - transformCache.up * _suspension.suspensionLength;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(fromPos, toPos);
            //draw suspension ends
            var right = transformCache.right;
            var fromEnd = fromPos - right * _suspension.suspensionLength / 10;
            var toEnd = fromPos + right * _suspension.suspensionLength / 10;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(fromEnd, toEnd);
            fromEnd = toPos - right * _suspension.suspensionLength / 10;
            toEnd = toPos + right * _suspension.suspensionLength / 10;
            Gizmos.DrawLine(fromEnd, toEnd);
            //place model in pos
            var wheelCenter = Vector3.Lerp(fromPos, toPos, _wheelPos);
            Gizmos.DrawWireSphere(wheelCenter, _suspension.suspensionLength / 20);
            if (wheelModel != null)
            {
                wheelModel.position = wheelCenter;
            }
            //draw wheel
            UnityEditor.Handles.color = Color.green;
            if (_wheel != null)
            {
                var radius = _wheel._wheel.radius;
                UnityEditor.Handles.DrawWireDisc(wheelCenter, transform.right, radius);
            }
            else
                _wheel = GetComponent<WheelAuthoring>();
        }
#endif
    }
}
