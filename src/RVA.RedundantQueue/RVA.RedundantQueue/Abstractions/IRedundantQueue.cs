using System;
using System.Threading.Tasks;
using RVA.RedundantQueue.Exceptions;

namespace RVA.RedundantQueue.Abstractions
{
    public interface IRedundantQueue<T> : ISubQueue<T>
    {
        EventHandler<RedundantQueueSendException<T>> OnError { get; set; }
        Task AddQueueAsync(ISubQueue<T> subQueue);
    }
}
