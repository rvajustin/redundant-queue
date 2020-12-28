using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using RVA.RedundantQueue.Abstractions;
using RVA.RedundantQueue.Exceptions;

namespace RVA.RedundantQueue.Implementations
{
    internal class RedundantQueue<T> : AbstractRedundantQueue, IRedundantQueue<T>
    {
        private readonly ConcurrentBag<ISubQueue<T>> _subQueues = new ConcurrentBag<ISubQueue<T>>();

        public RedundantQueue(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public (byte Priority, string Name)[] SubQueues => _subQueues.Select(q => (q.Priority, q.Name)).ToArray();
            
        public EventHandler<RedundantQueueSendException<T>> ErrorCallback { get; set; }

        public Task AddQueueAsync(ISubQueue<T> subQueue)
        {
            if (_subQueues.Any(q => q.Priority == subQueue.Priority))
            {
                throw new InvalidOperationException(
                    $"Cannot add two sub-queues to a redundant queue with the same priority.");
            }

            _subQueues.Add(subQueue);
            return Task.CompletedTask;
        }

        public async Task SendAsync(T message)
        {
            if (!_subQueues.Any())
            {
                throw new InvalidOperationException(
                    $"No sub-queues have been registered for `{Name}` of type `{typeof(T).FullName}`.");
            }

            var subQueues = _subQueues.OrderBy(q => q.Priority).ToArray();
            
            for (var i = 0; i < subQueues.Length; i++)
            {
                var subQueue = subQueues[i];

                if (subQueue == null)
                {
                    continue;
                }

                try
                {
                    await subQueue.SendAsync(message);
                    return;
                }
                catch (Exception ex)
                {
                    var lastChance = i + 1 == subQueues.Length;
                    var lastChanceMessage = lastChance
                        ? "This was the last chance to send the message."
                        : "Trying to send the message to a redundant queue.";

                    var e = new RedundantQueueSendException<T>(
                        $"Error while queuing message to redundant queue `{Name}` of type `{typeof(T).FullName}`.  " +
                        $"The message could not be sent to sub-queue named `{subQueue.Name}` in position number `{(i + 1)}`.  {lastChanceMessage}",
                        ex,
                        message,
                        subQueue);

                    ErrorCallback?.Invoke(this, e);

                    if (lastChance)
                    {
                        throw e;
                    }
                }
            }
        }
    }
}
