using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
[RequireComponent(typeof(WheelAuthoring))]
public class SuspensionAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private SuspensionComponent _suspension;
    [SerializeField] private Transform _wheelModel;

    private WheelAuthoring _wheel;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData<SuspensionComponent>(entity, _suspension);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        //draw suspension line
        Vector3 _fromPos = transform.position - (transform.up * _suspension.suspensionLength / 2);
        Vector3 _toPos = transform.position + (transform.up * _suspension.suspensionLength / 2);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(_fromPos, _toPos);
        //place model in pos
        Vector3 _wheelCenter = Vector3.Lerp(_fromPos, _toPos, _suspension.wheelPosition);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_wheelCenter, (_suspension.suspensionLength / 20));
        if (_wheelModel != null)
        {
            _wheelModel.position = _wheelCenter;
        }
        //draw wheel
        Gizmos.color = Color.green;
        if (_wheel != null)
        {
            float _radius = _wheel.wheel.wheelRadius;
            Gizmos.DrawWireSphere(_wheelCenter, _radius);
        }
        else
            _wheel = GetComponent<WheelAuthoring>();
    }
#endif
}
