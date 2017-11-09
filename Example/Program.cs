using Framework.Core.Configuration;
using Framework.Core.Infrastructure.DataStructures;
using LightInject;
using System.Diagnostics;
using Framework.Core.Repository;

namespace Example
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var container = new ServiceContainer();
            container.RegisterAssembly(typeof(Graph<>).Assembly);
            container.Register<ISomeConfigValue, AnotherImplementationOfConfigValue>(new PerScopeLifetime()); //override default registration
            container.Register<ClassThatDependsOnConfigValue, ClassThatDependsOnConfigValue>();

            using (container.BeginScope())
            {
                var config = container.GetInstance<ISomeConfigValue>();
                var configValue = config.GetValue();
                var service = container.GetInstance<ClassThatDependsOnConfigValue>();
                var serviceResult = service.GetResult();

                //Here we have another problem, I assumed that the proxy types are inherited by default from
                //target. But InvalidCastException is thrown if ISomeConfigValue doesn't expose
                //ICacheable interface even if concrete implementation does
                ((ICacheable)config).Refresh(); //someone change config value

                var serviceResult2 = service.GetResult();
                Debug.Assert(!serviceResult.Equals(serviceResult2));
            }
            
        }
    }
}