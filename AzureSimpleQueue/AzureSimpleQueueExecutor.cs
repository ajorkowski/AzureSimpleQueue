using System;
using System.Collections.Generic;

namespace AzureSimpleQueue
{
    public sealed class AzureSimpleQueueExecutor : ISimpleQueueExecutor, IDisposable
    {
        private readonly IEnumerable<ISimpleQueueListener> _listeners;
        private bool _isDisposed;

        public AzureSimpleQueueExecutor(IEnumerable<ISimpleQueueListener> listeners)
        {
            _listeners = listeners;
        }

        public void Start(int threadCount = 1)
        {
            if (_isDisposed) { throw new ObjectDisposedException("AzureSimpleQueueExecutor"); }

            foreach (var listener in _listeners)
            {
                listener.Listener.StartListener(threadCount);
            }
        }

        public void Stop()
        {
            if (_isDisposed) { throw new ObjectDisposedException("AzureSimpleQueueExecutor"); }

            foreach (var listener in _listeners)
            {
                listener.Listener.Stop();
            }
        }

        public void Dispose()
        {
            if(!_isDisposed)
            {
                Stop();
                _isDisposed = true;
            }
        }

        public IEnumerable<ISimpleQueueListener> Listeners
        {
            get { return _listeners; }
        }
    }
}
