using System;
using System.Threading;
using System.Threading.Tasks;
using RVA.RedundantQueue.Abstractions;

namespace RVA.RedundantQueue.Implementations
{
    public class UntetheredSubQueue<T> : ISubQueue<T>
    {
        private readonly Func<T, CancellationToken, Task> method;

        internal UntetheredSubQueue(string name, QueuePriority priority, Func<T, CancellationToken, Task> method)
        {
            this.method = method;
            Name = name;
            Priority = priority;
        }

        protected UntetheredSubQueue(string name, QueuePriority priority)
        {
            Name = name;
            Priority = priority;
        }

        public string Name { get; }
        public QueuePriority Priority { get; }

        public virtual async Task SendAsync(T message, CancellationToken cancellationToken)
        {
            if (method == null) return;

            await method.Invoke(message, cancellationToken);
        }
    }
}