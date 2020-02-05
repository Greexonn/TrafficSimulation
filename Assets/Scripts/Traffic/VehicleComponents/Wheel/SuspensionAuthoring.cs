using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
[RequireComponent(typeof(WheelAuthoring))]
public class SuspensionAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private SuspensionComponent _suspension;
    [SerializeField] private float _springStrengthKoef;
    [SerializeField] private float _damperStrengthKoef;
    [SerializeField] public Transform wheelModel;
    [SerializeField] [Range(0, 1)] private float _wheelPos;

    private WheelAuthoring _wheel;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var _vehicleMass = GetComponentInParent<Unity.Physics.Authoring.PhysicsBodyAuthoring>().Mass;

        _suspension.springStrength = _vehicleMass / 10 * _springStrengthKoef;
        _suspension.damperStrength = _vehicleMass / 10 * _damperStrengthKoef;

        dstManager.AddComponentData<SuspensionComponent>(entity, _suspension);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        //draw suspension line
        Vector3 _fromPos = transform.position;
        Vector3 _toPos = _fromPos - (transform.up * _suspension.suspensionLength);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(_fromPos, _toPos);
        //draw suspension ends
        Vector3 _fromEnd = _fromPos - (transform.right * _suspension.suspensionLength / 10);
        Vector3 _toEnd = _fromPos + (transform.right * _suspension.suspensionLength / 10);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_fromEnd, _toEnd);
        _fromEnd = _toPos - (transform.right * _suspension.suspensionLength / 10);
        _toEnd = _toPos + (transform.right * _suspension.suspensionLength / 10);
        Gizmos.DrawLine(_fromEnd, _toEnd);
        //place model in pos
        Vector3 _wheelCenter = Vector3.Lerp(_fromPos, _toPos, _wheelPos);
        Gizmos.DrawWireSphere(_wheelCenter, (_suspension.suspensionLength / 20));
        if (wheelModel != null)
        {
            wheelModel.position = _wheelCenter;
        }
        //draw wheel
        UnityEditor.Handles.color = Color.green;
        if (_wheel != null)
        {
            float _radius = _wheel.wheel.radius;
            UnityEditor.Handles.DrawWireDisc(_wheelCenter, transform.forward, _radius);
        }
        else
            _wheel = GetComponent<WheelAuthoring>();
    }
#endif
}
