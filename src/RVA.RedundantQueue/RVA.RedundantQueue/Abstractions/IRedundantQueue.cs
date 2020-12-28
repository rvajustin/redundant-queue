using System;
using System.Threading.Tasks;
using RVA.RedundantQueue.Exceptions;

namespace RVA.RedundantQueue.Abstractions
{
    public interface IRedundantQueue<T> 
    {
        EventHandler<RedundantQueueSendException<T>> ErrorCallback { get; set; }
        string Name { get; }
        (byte Priority, string Name)[] SubQueues { get; }
        Task AddQueueAsync(ISubQueue<T> subQueue);
        Task SendAsync(T message);
    }
}
