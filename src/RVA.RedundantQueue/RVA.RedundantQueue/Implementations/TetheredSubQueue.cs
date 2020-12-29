using System;
using System.Threading;
using System.Threading.Tasks;

namespace RVA.RedundantQueue.Implementations
{
    public class TetheredSubQueue<T, TTether> : UntetheredSubQueue<T>
    {
        private readonly Func<TTether, T, CancellationToken, Task> method;
        private readonly TTether tether;

        internal TetheredSubQueue(TTether tether, string name, QueuePriority priority,
            Func<TTether, T, CancellationToken, Task> method)
            : base(name, priority)
        {
            this.tether = tether;
            this.method = method;
        }

        public override async Task SendAsync(T message, CancellationToken cancellationToken)
        {
            if (method == null) return;

            await method.Invoke(tether, message, cancellationToken);
        }
    }
}