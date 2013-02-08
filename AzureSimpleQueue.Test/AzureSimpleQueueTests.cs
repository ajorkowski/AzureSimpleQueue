using System.Linq;
using Microsoft.Experience.CloudFx.Framework.Storage;
using NSubstitute;
using NUnit.Framework;
using System;
using AzureSimpleQueue.Test.Services;

namespace AzureSimpleQueue.Test
{
    [TestFixture]
    public class AzureSimpleQueueTests
    {
        protected ICloudQueueStorage _cloudQueue;
        protected AzureSimpleQueue<INotificationService> _queue;

        [SetUp]
        public void Init()
        {
            _cloudQueue = Substitute.For<ICloudQueueStorage>();
            _queue = new AzureSimpleQueue<INotificationService>(_cloudQueue);
        }

        [TestFixture]
        public class QueueMethod : AzureSimpleQueueTests
        {
            [Test]
            public void ValidServiceCallCreatesValidQueryMessage()
            {
                QueueMessage message = null;
                AttachNotificationServiceExpectation(q => { message = q; });

                _queue.Queue(s => s.DeleteAllNotifications(2));

                Assert.AreEqual("DeleteAllNotifications", message.Method);
                Assert.AreEqual("2", message.SerializedArguments.Single());
            }

            [Test]
            public void UsingVariableCreatesValidQueryMessage()
            {
                QueueMessage message = null;
                AttachNotificationServiceExpectation(q => { message = q; });

                var val = 4;
                _queue.Queue(s => s.DeleteAllNotifications(val));

                Assert.AreEqual("DeleteAllNotifications", message.Method);
                Assert.AreEqual("4", message.SerializedArguments.Single());
            }

            [Test]
            public void UsingLogicCreatesValidQueryMessage()
            {
                QueueMessage message = null;
                AttachNotificationServiceExpectation(q => { message = q; });

                var val = 4;
                _queue.Queue(s => s.DeleteAllNotifications(int.Parse(val.ToString())));

                Assert.AreEqual("DeleteAllNotifications", message.Method);
                Assert.AreEqual("4", message.SerializedArguments.Single());
            }

            [Test]
            public void MultipleArgumentsCreatesValidQueryMessage()
            {
                QueueMessage message = null;
                AttachNotificationServiceExpectation(q => { message = q; });

                _queue.Queue(s => s.CreateNotification(2, "someid", "some message"));

                var args = message.SerializedArguments.ToList();
                Assert.AreEqual("CreateNotification", message.Method);
                Assert.AreEqual("2", args[0]);
                Assert.AreEqual("\"someid\"", args[1]);
                Assert.AreEqual("\"some message\"", args[2]);
            }

            [Test]
            public void ComplexTypeArgumentsCreatesValidQueryMessage()
            {
                QueueMessage message = null;
                AttachNotificationServiceExpectation(q => { message = q; });

                _queue.Queue(s => s.CreateNotification(new ComplexType() 
                { 
                    UserId = 3, 
                    NotificationId = "someid", 
                    Message = "somemessage" 
                }));

                Assert.AreEqual("CreateNotification", message.Method);
                Assert.AreEqual("{\"UserId\":3,\"NotificationId\":\"someid\",\"Message\":\"somemessage\"}", message.SerializedArguments.Single());
            }

            [Test]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ExceptionWhenUsingTheServiceInTheArgs()
            {
                _queue.Queue(n => n.DeleteAllNotifications(int.Parse(n.ToString())));
            }

            [Test]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ExceptionWhenBaseObjectDoesntMatch()
            {
                _queue.Queue(n => _queue.ToString());
            }

            [Test]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ExceptionWhenCallingMethodWithReturnValue()
            {
                _queue.Queue(n => n.FakeService());
            }

            [Test]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ServiceMustHaveQueryAttribute()
            {
                new AzureSimpleQueue<IFakeService>(_cloudQueue);
            }

            private void AttachNotificationServiceExpectation(Action<QueueMessage> messageSave)
            {
                // We are overriding the base put definition - the extension methods call this
                _cloudQueue.Put("Notification", Arg.Any<IObservable<QueueMessage>>()).Returns(c =>
                {
                    var obs = (IObservable<QueueMessage>)c.Args()[1];
                    obs.Subscribe(messageSave);
                    return obs;
                });
            }
        }
    }
}
