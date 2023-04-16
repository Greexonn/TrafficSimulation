using Unity.Entities;
using Unity.Jobs;

namespace TrafficSimulation.Core.Systems
{
    public abstract partial class SystemWithPublicDependencyBase : SystemBase
    {
        public JobHandle PublicDependency => Dependency;
    }
}
