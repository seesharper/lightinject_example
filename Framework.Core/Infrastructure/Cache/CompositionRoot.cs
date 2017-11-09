using LightInject;

namespace Framework.Core.Infrastructure.Cache
{
    internal class CompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry registry)
        {
            registry.Register<IGlobalCache, GlobalCache>(new PerContainerLifetime());
        }
    }
}