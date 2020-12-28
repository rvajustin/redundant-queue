using System;
using System.Threading.Tasks;

namespace RVA.RedundantQueue.Implementations
{
    public class TetheredSubQueue<T, TTether> : UntetheredSubQueue<T>
    {
        private readonly TTether _tether;
        private readonly Func<TTether, T, Task> _method;

        internal TetheredSubQueue(TTether tether, string name, QueuePriority priority, Func<TTether, T, Task> method)
            : base(name, priority)
        {
            _tether = tether;
            _method = method;
        }

        public override async Task SendAsync(T message)
        {
            if (_method == null)
            {
                return;
            }

            await _method.Invoke(_tether, message);
        }
    }
}