using System;
using System.Linq;
using Framework.Core.Infrastructure.Cache;
using LightInject;

namespace Framework.Core.Repository
{
    class CompositionRoot: ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            serviceRegistry.Register<CacheableInterceptor, CacheableInterceptor>();
            serviceRegistry.Intercept(r => typeof(ICacheable).IsAssignableFrom(r.ImplementingType),
                (f, pd) => pd.Implement(f.GetInstance<CacheableInterceptor>,
                    m => m.DeclaringType == typeof(ICacheable)));

            serviceRegistry.Initialize(r => !typeof(IGlobalCache).IsAssignableFrom(r.ImplementingType), (f, obj) =>
            {
                var globalCache = f.GetInstance<IGlobalCache>();
                var cacheable = obj as ICacheable;

                /*Here all the magic should happen
                var dependent = initializationContext.Dependent; //the instance into which we are injecting obj param
                if(dependent != null) {
                    if(dependent is ICacheable || globalCache.ContainsDependency(dependent))
                        globalCache.RegisterRuntimeDependency(obj, dependent);
                }                
                ////////////////////////////////////////////////////////////*/
                if(cacheable != null) globalCache.AddCacheable(cacheable);
            });
        }
    }
}
