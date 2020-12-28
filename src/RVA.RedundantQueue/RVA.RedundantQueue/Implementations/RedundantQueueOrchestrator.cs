using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using RVA.RedundantQueue.Abstractions;
using RVA.RedundantQueue.Exceptions;

namespace RVA.RedundantQueue.Implementations
{
    internal class RedundantQueueOrchestrator : IRedundantQueueOrchestrator
    {
        private static readonly ConcurrentDictionary<(string, Type), int> Keys =
            new ConcurrentDictionary<(string, Type), int>();

        private static readonly ConcurrentDictionary<int, AbstractRedundantQueue> RedundantQueues =
            new ConcurrentDictionary<int, AbstractRedundantQueue>();

        public async Task<IRedundantQueue<T>> RegisterAsync<T>(string name, params ISubQueue<T>[] queues)
        {
            var redundantQueue = await CreateAsync<T>(name);
            
            foreach (var queue in queues)
            {
                await redundantQueue.AddQueueAsync(queue);
            }

            return redundantQueue;
        }

        public Task<IRedundantQueue<T>> CreateAsync<T>(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("`name` must be a valid, non-empty, non-null string.", nameof(name));
            }

            var key = GetKey<T>(name);

            if (RedundantQueues.ContainsKey(key))
            {
                throw new DuplicateKeyException($"The queue named `{name}` of type `{typeof(T).FullName}` has already been created.  Duplicates are not permitted.");
            }
            
            var redundantQueue = new RedundantQueue<T>(name);
            RedundantQueues[key] = redundantQueue;

            return Task.FromResult((IRedundantQueue<T>) redundantQueue);
        }

        public Task<IRedundantQueue<T>> ResolveAsync<T>(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("`name` must be a valid, non-empty, non-null string.", nameof(name));
            }

            var key = GetKey<T>(name);
            var redundantQueue = RedundantQueues[key];
            return Task.FromResult((IRedundantQueue<T>) redundantQueue);
        }

        private static int GetKey<T>(string name)
        {
            var key = (name, typeof(T));
            if (!Keys.ContainsKey(key))
            {
                unchecked
                {
                    var hash = name.Union(key.Item2.Name).Aggregate(23, (current, c) => current * 31 + c);
                    Keys[key] = hash;
                }
            }

            return Keys[key];
        }
    }
}