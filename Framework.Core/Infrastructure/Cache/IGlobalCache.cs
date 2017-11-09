using System;
using Framework.Core.Repository;

namespace Framework.Core.Infrastructure.Cache
{
    internal interface IGlobalCache
    {
        /// <summary>
        /// Store <see cref="WeakReference{T}"/> on <see cref="ICacheable"/> instance and call 
        /// <see cref="ICacheable.Refresh"/> on it
        /// during execution of <see cref="IGlobalCache.Refresh"/>
        /// </summary>
        /// <param name="cacheable"></param>
        void AddCacheable(ICacheable cacheable);

        /// <summary>
        /// So we could later determine correct refresh order durind
        /// <see cref="IGlobalCache.Refresh"/> execution.
        /// </summary>
        /// <param name="dependent"></param>
        /// <param name="dependency"></param>
        void RegisterRuntimeDependency(object dependency, object dependent);

        bool ContainsDependency(object dependency);

        /// <summary>
        /// In case if <see cref="ICacheable"/> customer call <see cref="ICacheable.Refresh"/> manually.
        /// We intercept <see cref="ICacheable.Refresh"/> method and call <see cref="IGlobalCache.NotifyRefresh"/>
        /// to refresh all <see cref="ICacheable"/> instances which depends on this <see cref="ICacheable"/> intance.
        /// </summary>
        /// <param name="cacheable"></param>
        void NotifyRefresh(ICacheable cacheable);

        void Refresh();
    }
}
