namespace RVA.RedundantQueue.Abstractions
{
    public interface IRedundantQueueSendException<out T>
    {
        T QueueMessage { get; }
    }
}