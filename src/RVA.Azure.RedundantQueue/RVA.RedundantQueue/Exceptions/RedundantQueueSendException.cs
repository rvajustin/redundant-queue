using System;
using RVA.RedundantQueue.Abstractions;

namespace RVA.RedundantQueue.Exceptions
{
    public class RedundantQueueSendException<T> : Exception, IRedundantQueueSendException<T>
    {
        public T QueueMessage { get; }

        public RedundantQueueSendException(string message, Exception innerException, T queueMessage) : base(message, innerException)
        {
            QueueMessage = queueMessage;
        }
    }
}