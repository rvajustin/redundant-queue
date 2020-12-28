using System;
using System.Threading.Tasks;
using RVA.RedundantQueue.Abstractions;

namespace RVA.RedundantQueue.Implementations
{
    public static class SubQueue
    {
        public static ISubQueue<T> Create<T>(string name, QueuePriority priority, Func<T, Task> method)
        {
            return new UntetheredSubQueue<T>(name, priority, method);
        }

        public static ISubQueue<T> Create<T>(string name, QueuePriority priority, Action<T> method)
        {
            Task SendMessageAsync(T message)
            {
                method(message);
                return Task.CompletedTask;
            }

            return new UntetheredSubQueue<T>(name, priority, SendMessageAsync);
        }

        public static ISubQueue<T> CreateWithTether<T, TTether>(TTether value, string name, QueuePriority priority,
            Func<TTether, T, Task> method)
        {
            return new TetheredSubQueue<T, TTether>(value, name, priority, method);
        }

        public static ISubQueue<T> CreateWithTether<T, TTether>(TTether value, string name, QueuePriority priority, Action<TTether, T> method)
        {
            Task SendMessageAsync(TTether inner, T message)
            {
                method(inner, message);
                return Task.CompletedTask;
            }

            return new TetheredSubQueue<T, TTether>(value, name, priority, SendMessageAsync);
        }
    }
}