using System.Collections.Generic;

namespace AzureSimpleQueue
{
    public interface ISimpleQueueExecutor
    {
        IEnumerable<ISimpleQueueListener> Listeners { get; }

        void Start(int threadCount = 1);
        void Stop();
    }
}
