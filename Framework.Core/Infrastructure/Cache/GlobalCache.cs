using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Core.Infrastructure.DataStructures;
using Framework.Core.Repository;

namespace Framework.Core.Infrastructure.Cache
{
    internal class GlobalCache : IGlobalCache
    {
        private readonly Graph<CustomWeakReference> _dependencyGraph
            = new Graph<CustomWeakReference>();

        private readonly object _syncRoot = new object();

        private HashSet<CustomWeakReference> _refreshableSet =
            new HashSet<CustomWeakReference>();

        private ICacheable _refreshing;

        void IGlobalCache.RegisterRuntimeDependency(object dependency, object dependent)
        {
            if (dependency == this || dependent == this)
                throw new InvalidOperationException("This should never happen");
            _dependencyGraph.AddEdge(new CustomWeakReference(dependency),
                new CustomWeakReference(dependent));
        }

        bool IGlobalCache.ContainsDependency(object dependency)
        {
            return _dependencyGraph.ContainsNode(new CustomWeakReference(dependency));
        }

        void IGlobalCache.AddCacheable(ICacheable item)
        {
            var weakReference = new CustomWeakReference(item);
            //AssertTrackingDependencies(weakReference);
            lock (_syncRoot)
            {
                _refreshableSet.Add(weakReference);
            }
        }

        void IGlobalCache.Refresh()
        {
            Refersh(null);
        }

        void IGlobalCache.NotifyRefresh(ICacheable cacheable)
        {
            Refersh(cacheable);
        }

        private void AssertTrackingDependencies(CustomWeakReference item)
        {
            if (!_dependencyGraph.ContainsNode(item))
                throw new InvalidOperationException($"Missing info about {nameof(ICacheable)} instance dependencies");
        }

        private void Refersh(ICacheable dependency)
        {
            lock (_syncRoot)
            {
                if (_refreshing == dependency) return;

                var list = new List<ICacheable>();
                var refreshableSet = new HashSet<CustomWeakReference>();
                foreach (var wr in _refreshableSet)
                {
                    var target = (ICacheable) wr.Target;
                    if (target == null) continue;
                    list.Add(target);
                    refreshableSet.Add(wr);
                }
                _refreshableSet = refreshableSet;
                var weakCollection = _dependencyGraph.TopologicalSortedCollection;
                foreach (var wr in weakCollection)
                    if (wr.Target == null) _dependencyGraph.RemoveNode(wr);
                var dependencies = weakCollection
                    .Select(z => z.Target as ICacheable)
                    .Where(z => z != null)
                    .ToList();
                var toRefresh = list
                    .OrderBy(z => dependencies.FindIndex(x => object.ReferenceEquals(x, z)))
                    .ToList();
                if (dependency != null)
                {
                    var index = toRefresh.FindIndex(z => object.ReferenceEquals(z, dependency));
                    if(index < 0)
                        throw new InvalidOperationException("Why it happens?");
                    toRefresh = toRefresh
                        .Skip(index + 1)
                        .ToList();
                }
                try
                {
                    foreach (var cacheable in toRefresh)
                    {
                        _refreshing = cacheable;
                        cacheable.Refresh();
                    }
                }
                finally
                {
                    _refreshing = null;
                }
            }
        }

        private class CustomWeakReference
        {
            private readonly int _hashCode;
            private readonly WeakReference _ref;

            public CustomWeakReference(object target)
            {
                _ref = new WeakReference(target);
                _hashCode = target.GetHashCode();
            }

            public object Target => _ref.Target;

            public override int GetHashCode()
            {
                return _hashCode;
            }

            public override bool Equals(object obj)
            {
                var weakReference = obj as CustomWeakReference;
                return weakReference != null
                       && object.ReferenceEquals(weakReference.Target, Target);
            }
        }
    }
}