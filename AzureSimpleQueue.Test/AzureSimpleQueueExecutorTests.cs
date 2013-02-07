using System;
using System.Collections.Generic;
using Microsoft.Experience.CloudFx.Extensions.Storage;
using NSubstitute;
using NUnit.Framework;

namespace AzureSimpleQueue.Test
{
    [TestFixture]
    public class AzureSimpleQueueExecutorTests
    {
        protected IEnumerable<ISimpleQueueListener> _listeners; 
        protected AzureSimpleQueueExecutor _executor;

        [SetUp]
        public void Init()
        {
            _listeners = new List<ISimpleQueueListener>
            {
                MockListener(),
                MockListener(),
                MockListener()
            };

            _executor = new AzureSimpleQueueExecutor(_listeners);
        }

        private ISimpleQueueListener MockListener()
        {
            var listener = Substitute.For<ISimpleQueueListener>();
            listener.Listener.Returns(Substitute.For<ICloudQueueListenerExtension<QueueMessage>>());
            return listener;
        }

        [TestFixture]
        public class StartServicesMethod: AzureSimpleQueueExecutorTests
        {
            [Test]
            public void ExecutesStartOnEachListener()
            {
                _executor.StartServices(4);

                foreach (var listener in _listeners)
                {
                    listener.Listener.Received(1).StartListener(4);
                }
            }

            [Test]
            [ExpectedException(typeof(ObjectDisposedException))]
            public void ExceptionWhenCalledWhenDisposed()
            {
                _executor.Dispose();
                _executor.StartServices();
            }
        }

        [TestFixture]
        public class StopServicesMethod : AzureSimpleQueueExecutorTests
        {
            [Test]
            public void ExecutesStopOnEachListener()
            {
                _executor.StopServices();

                foreach (var listener in _listeners)
                {
                    listener.Listener.Received(1).Stop();
                }
            }

            [Test]
            [ExpectedException(typeof(ObjectDisposedException))]
            public void ExceptionWhenCalledWhenDisposed()
            {
                _executor.Dispose();
                _executor.StopServices();
            }
        }
    }
}
