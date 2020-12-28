using System;
using System.Threading.Tasks;
using RVA.Azure.RedundantQueue.Exceptions;

namespace RVA.Azure.RedundantQueue.Abstractions
{
    public interface IRedundantQueue<T> : ISubQueue<T>
    {
        EventHandler<RedundantQueueSendException<T>> OnError { get; set; }
        Task AddQueueAsync(ISubQueue<T> subQueue);
    }
}
