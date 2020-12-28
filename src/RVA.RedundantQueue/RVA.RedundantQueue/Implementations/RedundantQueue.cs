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
        private readonly ConcurrentBag<ISubQueue<T>> _queues = new ConcurrentBag<ISubQueue<T>>();

        public RedundantQueue(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public EventHandler<RedundantQueueSendException<T>> OnError { get; set; }

        public Task AddQueueAsync(ISubQueue<T> subQueue)
        {
            _queues.Add(subQueue);
            return Task.CompletedTask;
        }

        public async Task SendAsync(T message)
        {
            if (_queues.Any())
            {
                throw new InvalidOperationException(
                    $"No sub-queues have been registered for `{Name}` of type `{typeof(T).FullName}`.");
            }

            for (var i = 0; i < _queues.Count; i++)
            {
                var queue = _queues.ElementAtOrDefault(i);

                if (queue == null)
                {
                    continue;
                }

                try
                {
                    await queue.SendAsync(message);
                    return;
                }
                catch (Exception ex)
                {
                    var lastChance = i + 1 == _queues.Count;
                    var lastChanceMessage = lastChance
                        ? "This was the last chance to send the message."
                        : "Trying to send the message to a redundant queue.";

                    var e = new RedundantQueueSendException<T>(
                        $"Error while queuing message to redundant queue `{Name}` of type `{typeof(T).FullName}`.  " +
                        $"The message could not be sent to sub-queue named `{queue.Name}` in position number `{i}`.  {lastChanceMessage}",
                        ex,
                        message);

                    OnError?.Invoke(this, e);

                    if (lastChance)
                    {
                        throw e;
                    }
                }

                i++;
            }
        }
    }
}
