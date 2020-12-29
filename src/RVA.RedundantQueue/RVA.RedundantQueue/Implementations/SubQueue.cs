using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RVA.RedundantQueue.Abstractions;

namespace RVA.RedundantQueue.Implementations
{
    public static class SubQueue
    {
        static SubQueue()
        {
        }

        private static IDictionary<(string, Type), int> HashCache { get; } =
            new ConcurrentDictionary<(string, Type), int>();

        public static ISubQueue<T> CreateForAsyncProcessor<T>(string name, QueuePriority priority,
            Func<T, CancellationToken, Task> method)
        {
            return new UntetheredSubQueue<T>(name, priority, method);
        }

        public static ISubQueue<T> Create<T>(string name, QueuePriority priority, Action<T> method)
        {
            Task SendMessageAsync(T message, CancellationToken cancellationToken)
            {
                method(message);
                return Task.CompletedTask;
            }

            return new UntetheredSubQueue<T>(name, priority, SendMessageAsync);
        }

        public static ISubQueue<T> CreateForAsyncProcessorWithTether<T, TTether>(TTether value, string name,
            QueuePriority priority,
            Func<TTether, T, CancellationToken, Task> method)
        {
            return new TetheredSubQueue<T, TTether>(value, name, priority, method);
        }

        public static ISubQueue<T> CreateWithTether<T, TTether>(TTether value, string name, QueuePriority priority,
            Action<TTether, T> method)
        {
            Task SendMessageAsync(TTether inner, T message, CancellationToken cancellationToken)
            {
                method(inner, message);
                return Task.CompletedTask;
            }

            return new TetheredSubQueue<T, TTether>(value, name, priority, SendMessageAsync);
        }

        internal static int GetHash<T>(string name)
        {
            var key = (name, typeof(T));

            if (HashCache.ContainsKey(key)) return HashCache[key];

            unchecked
            {
                var hash = name.Aggregate(23, (current, c) => current * 31 + c);
                hash = key.Item2.Name.Aggregate(hash, (current, c) => current * 31 + c);
                HashCache[key] = hash;

                return hash;
            }
        }
    }
}