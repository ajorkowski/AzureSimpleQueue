using System.Linq;
using Microsoft.Experience.CloudFx.Framework.Storage;
using NSubstitute;
using NUnit.Framework;
using System;
using AzureSimpleQueue.Test.Services;

namespace AzureSimpleQueue.Test
{
    [TestFixture]
    public class AzureSimpleQueueProducerTest
    {
        protected ICloudQueueStorage _cloudQueue;
        protected AzureSimpleQueueProducer _producer;

        [SetUp]
        public void Init()
        {
            _cloudQueue = Substitute.For<ICloudQueueStorage>();
            _producer = new AzureSimpleQueueProducer(_cloudQueue);
        }

        [TestFixture]
        public class QueueMethod : AzureSimpleQueueProducerTest
        {
            [Test]
            public void ValidServiceCallCreatesValidQueryMessage()
            {
                QueryMessage message = null;
                _cloudQueue.Put("Notification", Arg.Any<IObservable<QueryMessage>>()).Returns(c =>
                {
                    var obs = (IObservable<QueryMessage>)c.Args()[1];
                    obs.Subscribe(q => { message = q; });
                    return null;
                });

                _producer.Queue<INotificationService>(s => s.DeleteAllNotifications(2));

                Assert.AreEqual("DeleteAllNotifications", message.Method);
                Assert.AreEqual("2", message.SerializedArguments.Single());
            }

            [Test]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ServiceMustHaveQueryAttribute()
            {
                _producer.Queue<IFakeService>(s => s.SomeMethod());
            }
        }
    }
}
