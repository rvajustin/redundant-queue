namespace RVA.RedundantQueue.Abstractions
{
    public interface IReadOnlySubQueueConfiguration
    {
        uint RetryCount { get; }
    }
}