namespace RVA.RedundantQueue.Abstractions
{
    public interface ISubQueueConfiguration : IReadOnlySubQueueConfiguration
    {
        new uint RetryCount { get; set; }
    }
}