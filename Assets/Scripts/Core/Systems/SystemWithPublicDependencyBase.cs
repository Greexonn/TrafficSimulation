using Unity.Entities;
using Unity.Jobs;

namespace Core.Systems
{
    public abstract class SystemWithPublicDependencyBase : SystemBase
    {
        public JobHandle PublicDependency => Dependency;
    }
}
