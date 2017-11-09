using LightInject;

namespace Framework.Core.Configuration
{
    internal class CompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            serviceRegistry.Register<ISomeConfigValue, ConfigValue>();
        }
    }
}