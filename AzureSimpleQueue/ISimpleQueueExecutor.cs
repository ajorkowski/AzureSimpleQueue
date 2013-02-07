using System.Collections.Generic;

namespace AzureSimpleQueue
{
    public interface ISimpleQueueExecutor
    {
        IEnumerable<ISimpleQueueListener> Listeners { get; }

        void StartServices(int threadCount = 1);
        void StopServices();
    }
}
