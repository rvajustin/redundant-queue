using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RVA.RedundantQueue.Abstractions;
using RVA.RedundantQueue.Exceptions;

namespace RVA.RedundantQueue.Implementations
{
    internal class RedundantQueue<T> : AbstractRedundantQueue, IRedundantQueue<T>
    {
        private readonly IDictionary<int, ISubQueueConfiguration> configurations =
            new ConcurrentDictionary<int, ISubQueueConfiguration>();

        private readonly ConcurrentBag<ISubQueue<T>> subQueues = new ConcurrentBag<ISubQueue<T>>();

        public RedundantQueue(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public IEnumerable<SubQueueMetadata> SubQueues =>
            subQueues.Select(q => new SubQueueMetadata(q.Name, q.Priority)).ToArray();

        public EventHandler<RedundantQueueSendException<T>> ErrorCallback { get; set; }

        public QueueContext<T> AddSubQueue(ISubQueue<T> subQueue)
        {
            if (subQueues.Any(q => Equals(q.Priority, subQueue.Priority)))
            {
                throw new InvalidOperationException(
                    "Cannot add two sub-queues to a redundant queue with the same priority.");
            }

            subQueues.Add(subQueue);
            return new QueueContext<T>.FromSources
            {
                RedundantQueue = this,
                SubQueue = subQueue
            };
        }

        public async Task SendAsync(T message)
        {
            if (!subQueues.Any())
            {
                throw new InvalidOperationException(
                    $"No sub-queues have been registered for `{Name}` of type `{typeof(T).FullName}`.");
            }

            var queues = subQueues.OrderBy(q => q.Priority).ToArray();

            for (var iteration = 0; iteration < queues.Length; iteration++)
            {
                var subQueue = GetSubQueue(queues, iteration, out var retriesRemaining);
                var success = await SendAsync(message, retriesRemaining, subQueue, iteration, queues);
                if (success)
                {
                    return;
                }
            }
        }

        private async Task<bool> SendAsync(T message, int retriesRemaining, ISubQueue<T> subQueue, int iteration, IReadOnlyCollection<ISubQueue<T>> queues)
        {
            while (retriesRemaining >= 0)
            {
                retriesRemaining--;

                try
                {
                    var ok = await SendAsync(subQueue, message);

                    if (ok)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    CheckForLastChanceException(message, iteration, queues, retriesRemaining, subQueue, ex);
                }
            }

            return false;
        }

        private static async Task<bool> SendAsync(ISubQueue<T> subQueue, T message)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            await subQueue.SendAsync(message, cancellationToken);

            return true;
        }

        private ISubQueue<T> GetSubQueue(ISubQueue<T>[] queues, int iteration, out int retriesRemaining)
        {
            var subQueue = queues[iteration];

            var key = SubQueue.GetHash<T>(subQueue.Name);
            var configuration = configurations.ContainsKey(key)
                ? configurations[key]
                : SubQueueConfiguration.Default;
            retriesRemaining = unchecked((int) configuration.RetryCount);
            return subQueue;
        }

        private void CheckForLastChanceException(T message, int iteration, IReadOnlyCollection<ISubQueue<T>> queues, int retriesRemaining, ISubQueue<T> subQueue,
            Exception ex)
        {
            var lastChance = iteration + 1 == queues.Count && retriesRemaining == 0;
            var lastChanceMessage = lastChance
                ? "This was the last chance to send the message."
                : "Trying to send the message to a redundant queue.";

            var e = new RedundantQueueSendException<T>(
                $"Error while queuing message to redundant queue `{Name}` of type `{typeof(T).FullName}`.  " +
                $"The message could not be sent to sub-queue named `{subQueue.Name}` in position number `{iteration + 1}`.  {lastChanceMessage}",
                ex,
                message,
                subQueue);

            ErrorCallback?.Invoke(this, e);

            if (lastChance)
            {
                throw e;
            }
        }

        public IRedundantQueue<T> Configure(ISubQueue<T> subQueue, Action<ISubQueueConfiguration> callback)
        {
            var key = SubQueue.GetHash<T>(subQueue.Name);

            if (!configurations.ContainsKey(key))
            {
                var configuration = new SubQueueConfiguration();
                configurations[key] = configuration;
            }

            callback(configurations[key]);
            return this;
        }
    }
}