using RVA.RedundantQueue.Abstractions;

namespace RVA.RedundantQueue.Implementations
{
    internal struct SubQueueConfiguration : ISubQueueConfiguration
    {
        internal static readonly IReadOnlySubQueueConfiguration Default = new SubQueueConfiguration
        {
            RetryCount = 0
        };

        public uint RetryCount { get; set; }
    }
}