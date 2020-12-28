namespace RVA.RedundantQueue.Abstractions
{
    public interface IRedundantQueueSendException<T>
    {
        T QueueMessage { get; }
        ISubQueue<T> SubQueue { get; }
    }
}