namespace RVA.RedundantQueue.Implementations
{
    public readonly struct SubQueueMetadata
    {
        public string Name { get; }
        public QueuePriority Priority { get; }
        
        public SubQueueMetadata( string name, QueuePriority priority)
        {
            Priority = priority;
            Name = name;
        }
    }
}