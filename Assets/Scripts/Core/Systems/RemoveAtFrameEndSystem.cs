using Core.Components;
using Unity.Collections;
using Unity.Entities;

namespace Core.Systems
{
    [UpdateInGroup(typeof(FinishingSystemGroup))]
    public class RemoveAtFrameEndSystem : SystemBase
    {
        private NativeArray<ComponentType> _typesToRemove;

        protected override void OnStartRunning()
        {
            var typesList = EntityManager.GetAssignableComponentTypes(typeof(IRemoveAtFrameEndComponent));
            _typesToRemove = new NativeArray<ComponentType>(typesList.Count, Allocator.Persistent);
            for (var i = 0; i < typesList.Count; i++)
            {
                _typesToRemove[i] = new ComponentType(typesList[i], ComponentType.AccessMode.ReadOnly);
            }
        }

        protected override void OnStopRunning()
        {
            _typesToRemove.Dispose();
        }

        protected override void OnUpdate()
        {
            foreach (var componentType in _typesToRemove)
            {
                EntityManager.RemoveComponent(GetEntityQuery(componentType), componentType);
            }
        }
    }
}
