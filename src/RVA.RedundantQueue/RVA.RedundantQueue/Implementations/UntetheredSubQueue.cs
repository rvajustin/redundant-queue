using System;
using System.Threading.Tasks;
using RVA.RedundantQueue.Abstractions;

namespace RVA.RedundantQueue.Implementations
{
    public class UntetheredSubQueue<T> : ISubQueue<T>
    {
        private readonly Func<T, Task> _method;
        public string Name { get; }
        public QueuePriority Priority { get; }

        internal UntetheredSubQueue(string name, QueuePriority priority, Func<T, Task> method)
        {
            _method = method;
            Name = name;
            Priority = priority;
        }
        
        protected internal UntetheredSubQueue(string name, QueuePriority priority)
        {
            Name = name;
            Priority = priority;
        }

        public virtual async Task SendAsync(T message)
        {
            if (_method == null)
            {
                return;
            }

            await _method.Invoke(message);
        }
    }
}
