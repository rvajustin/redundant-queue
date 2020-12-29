using System;

namespace RVA.RedundantQueue.Abstractions
{
    public abstract class QueueContext<T>
    {
        public IRedundantQueue<T> RedundantQueue { get; set; }
        public ISubQueue<T> SubQueue { get; set; }
        
        internal class FromSources : QueueContext<T>
        {
             
        }

        public QueueContext<T> Configure(Action<ISubQueueConfiguration> callback)
        {
            RedundantQueue.Configure(SubQueue, callback);
            return this;
        }
    }
}