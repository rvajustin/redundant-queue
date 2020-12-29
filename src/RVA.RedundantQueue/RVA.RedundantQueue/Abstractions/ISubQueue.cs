using System.Threading;
using System.Threading.Tasks;
using RVA.RedundantQueue.Implementations;

namespace RVA.RedundantQueue.Abstractions
{
    public interface ISubQueue<in T>
    {
        string Name { get; }
        QueuePriority Priority { get; }
        Task SendAsync(T message, CancellationToken cancellationToken);
    }
}