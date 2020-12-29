using System;
using RVA.RedundantQueue.Abstractions;

namespace RVA.RedundantQueue.Exceptions
{
    public class RedundantQueueSendException<T> : Exception, IRedundantQueueSendException<T>
    {
        public RedundantQueueSendException(string message, Exception innerException, T queueMessage,
            ISubQueue<T> subQueue)
            : base(message, innerException)
        {
            QueueMessage = queueMessage;
            SubQueue = subQueue;
        }

        public T QueueMessage { get; }

        public ISubQueue<T> SubQueue { get; }
    }
}