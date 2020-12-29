using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using RVA.RedundantQueue.Abstractions;
using RVA.RedundantQueue.Exceptions;

namespace RVA.RedundantQueue.Implementations
{
    public class RedundantQueueOrchestrator : IRedundantQueueOrchestrator
    {
        private static readonly ConcurrentDictionary<int, AbstractRedundantQueue> RedundantQueues =
            new ConcurrentDictionary<int, AbstractRedundantQueue>();

        public async Task<IRedundantQueue<T>> RegisterAsync<T>(string name, params ISubQueue<T>[] queues)
        {
            var redundantQueue = await CreateAsync<T>(name);

            foreach (var queue in queues)
            {
                redundantQueue.AddSubQueue(queue);
            }

            return redundantQueue;
        }

        public Task<IRedundantQueue<T>> CreateAsync<T>(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("`name` must be a valid, non-empty, non-null string.", nameof(name));
            }

            var key = SubQueue.GetHash<T>(name);

            if (RedundantQueues.ContainsKey(key))
            {
                throw new DuplicateKeyException(
                    $"The queue named `{name}` of type `{typeof(T).FullName}` has already been created.  Duplicates are not permitted.");
            }

            var redundantQueue = new RedundantQueue<T>(name);
            RedundantQueues[key] = redundantQueue;

            return Task.FromResult((IRedundantQueue<T>) redundantQueue);
        }

        public Task<IRedundantQueue<T>> ResolveAsync<T>(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("`name` must be a valid, non-empty, non-null string.", nameof(name));
            }

            var key = SubQueue.GetHash<T>(name);

            if (!RedundantQueues.ContainsKey(key))
            {
                throw new KeyNotFoundException(
                    $"A queue named `{name}` of type `{typeof(T).FullName}` could not be located.");
            }

            var redundantQueue = RedundantQueues[key];
            return Task.FromResult((IRedundantQueue<T>) redundantQueue);
        }
    }
}