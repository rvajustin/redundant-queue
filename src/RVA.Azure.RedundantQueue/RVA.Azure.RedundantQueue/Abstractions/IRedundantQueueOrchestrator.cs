using System.Threading.Tasks;

namespace RVA.Azure.RedundantQueue.Abstractions
{
    public interface IRedundantQueueOrchestrator
    {
        Task<IRedundantQueue<T>> CreateAsync<T>(string name);
        Task<IRedundantQueue<T>> RegisterAsync<T>(string name, params ISubQueue<T>[] queues);
        Task<IRedundantQueue<T>> ResolveAsync<T>(string name);
    }
}
