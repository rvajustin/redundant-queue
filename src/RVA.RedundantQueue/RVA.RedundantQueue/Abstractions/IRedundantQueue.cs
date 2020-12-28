using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RVA.RedundantQueue.Exceptions;
using RVA.RedundantQueue.Implementations;

namespace RVA.RedundantQueue.Abstractions
{
    public interface IRedundantQueue<T> 
    {
        EventHandler<RedundantQueueSendException<T>> ErrorCallback { get; set; }
        string Name { get; }
        IEnumerable<SubQueueMetadata> SubQueues { get; }
        Task AddQueueAsync(ISubQueue<T> subQueue);
        Task SendAsync(T message);
    }
}
