using System;
using Framework.Core.Infrastructure.Cache;
using LightInject.Interception;

namespace Framework.Core.Repository
{
    internal class CacheableInterceptor : IInterceptor
    {
        private readonly Func<IGlobalCache> _globalCacheFactory;

        public CacheableInterceptor(Func<IGlobalCache> globalCacheFactory)
        {
            _globalCacheFactory = globalCacheFactory;
        }

        public object Invoke(IInvocationInfo invocationInfo)
        {
            var returnValue = invocationInfo.Proceed();
            var globalCache = _globalCacheFactory.Invoke();
            globalCache.NotifyRefresh((ICacheable) invocationInfo.Proxy);
            return returnValue;
        }
    }
}