using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Core.Infrastructure.DataStructures
{
    public class Graph<T>
    {
        private readonly ConcurrentDictionary<T, HashSet<T>> _nodeLinks = new ConcurrentDictionary<T, HashSet<T>>();
        private volatile Lazy<IReadOnlyCollection<T>> _sorted;

        public Graph()
        {
            _sorted = new Lazy<IReadOnlyCollection<T>>(GetSortedArray);
        }

        /// <exception cref="InvalidOperationException"></exception>
        public IReadOnlyCollection<T> TopologicalSortedCollection => _sorted.Value;

        public void AddNode(T node)
        {
            _nodeLinks.GetOrAdd(node, n => new HashSet<T>());
        }

        public void AddEdge(T nodeFrom, T nodeTo)
        {
            _nodeLinks.GetOrAdd(nodeTo, n => new HashSet<T>());
            var links = _nodeLinks.GetOrAdd(nodeFrom, n => new HashSet<T>());
            lock (links)
            {
                links.Add(nodeTo);
            }
            _sorted = new Lazy<IReadOnlyCollection<T>>(GetSortedArray);
        }

        public void RemoveNode(T node, bool includeDescendants = true)
        {
            HashSet<T> links;
            _nodeLinks.TryRemove(node, out links);
            if (links == null) return;
            lock (links)
            {
                foreach (var kv in _nodeLinks)
                    lock (kv.Value)
                    {
                        kv.Value.Remove(node);
                        if (includeDescendants
                            && links.Contains(kv.Key))
                            RemoveNode(kv.Key, true);
                    }
            }
            _sorted = new Lazy<IReadOnlyCollection<T>>(GetSortedArray);
        }

        public void RemoveEdge(T nodeFrom, T nodeTo)
        {
            HashSet<T> links;
            _nodeLinks.TryGetValue(nodeFrom, out links);
            if (links == null) return;
            lock (links)
            {
                RemoveNode(nodeTo);
            }
            _sorted = new Lazy<IReadOnlyCollection<T>>(GetSortedArray);
        }

        public bool ContainsNode(T node)
        {
            return _nodeLinks.ContainsKey(node);
        }

        public bool ContainsEdge(T nodeFrom, T nodeTo)
        {
            HashSet<T> links;
            if (!_nodeLinks.TryGetValue(nodeFrom, out links)) return false;
            lock (links)
            {
                return links.Contains(nodeTo);
            }
        }

        public IReadOnlyCollection<T> GetDescendantNodes(T node)
        {
            var nodes = new HashSet<T>();
            FillDescendantNodes(node, nodes);
            return nodes.ToArray();
        }

        private void FillDescendantNodes(T node, HashSet<T> nodes)
        {
            HashSet<T> links;
            _nodeLinks.TryGetValue(node, out links);
            if(links == null) return;
            lock (links)
            {
                foreach (var n in links)
                {
                    if(!nodes.Add(n)) continue;
                    FillDescendantNodes(n, nodes);
                }
            }
        }

        private T[] GetSortedArray()
        {
            var stack = new Stack<T>();
            var visited = new HashSet<T>();
            var recStack = new Stack<T>();
            foreach (var node in _nodeLinks.Keys)
                DFS(node, visited, recStack, stack);
            return stack.ToArray();
        }

        private void DFS(T node, HashSet<T> visited, Stack<T> recStack, Stack<T> stack)
        {
            if (visited.Contains(node)) return;
            recStack.Push(node);
            visited.Add(node);
            var links = _nodeLinks[node];
            lock (links)
            {
                foreach (var n in links)
                {
                    if (!visited.Contains(n))
                        DFS(n, visited, recStack, stack);
                    if (!recStack.Contains(n)) continue;
                    var strLoop = string.Join(" <= ", recStack.ToArray());
                    throw new InvalidOperationException($"There was a loop detected in the graph. {strLoop} <= {node}");
                }
            }
            stack.Push(node);
            recStack.Pop();
        }
    }
}