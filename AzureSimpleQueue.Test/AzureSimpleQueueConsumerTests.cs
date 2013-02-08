using System;
using System.Linq;
using AzureSimpleQueue.Test.Services;
using NSubstitute;
using NUnit.Framework;
using Microsoft.Experience.CloudFx.Framework.Extensibility;

namespace AzureSimpleQueue.Test
{
    [TestFixture]
    public class AzureSimpleQueueConsumerTests
    {
        protected AzureSimpleQueueConsumer _consumer;

        [SetUp]
        public void Init()
        {
            _consumer = new AzureSimpleQueueConsumer(Substitute.For<IExtensibleComponent>());
        }

        [TestFixture]
        public class AttachQueuedServicesMethod : AzureSimpleQueueConsumerTests
        {
            [Test]
            public void AttachingRealServiceUsesCorrectQueueLocation()
            {
                var executor = _consumer.AttachQueuedServices("connectionstring", Substitute.For<INotificationService>());

                var queueLoc = executor.Listeners.Single().Listener.QueueLocation;

                Assert.AreEqual("Notification", queueLoc.QueueName);
                Assert.AreEqual("connectionstring", queueLoc.StorageAccount);
            }

            [Test]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ExceptionWhenNoServices()
            {
                _consumer.AttachQueuedServices("connectionstring", new object[] {});
            }

            [Test]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ExceptionWhenUsingServiceWithNoAttribute()
            {
                _consumer.AttachQueuedServices("connectionstring", Substitute.For<IFakeService>());
            }
        }
    }
}
