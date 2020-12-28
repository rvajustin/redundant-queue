namespace RVA.Azure.RedundantQueue.Abstractions
{
    public interface IRedundantQueueSendException<out T>
    {
        T QueueMessage { get; }
    }
}