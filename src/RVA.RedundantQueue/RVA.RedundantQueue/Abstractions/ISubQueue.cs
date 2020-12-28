using System.Threading.Tasks;

namespace RVA.RedundantQueue.Abstractions
{
    public interface ISubQueue<in T>
    {
        string Name { get; }
        Task SendAsync(T message);
    }
}
