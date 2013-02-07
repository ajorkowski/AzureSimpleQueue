using System.Linq;
using Microsoft.Experience.CloudFx.Framework.Storage;
using NSubstitute;
using NUnit.Framework;
using System;
using AzureSimpleQueue.Test.Services;

namespace AzureSimpleQueue.Test
{
    [TestFixture]
    public class AzureSimpleQueueProducerTests
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
        public class QueueMethod : AzureSimpleQueueProducerTests
        {
            [Test]
            public void ValidServiceCallCreatesValidQueryMessage()
            {
                QueueMessage message = null;
                AttachNotificationServiceExpectation(q => { message = q; });

                _producer.Queue<INotificationService>(s => s.DeleteAllNotifications(2));

                Assert.AreEqual("DeleteAllNotifications", message.Method);
                Assert.AreEqual("2", message.SerializedArguments.Single());
            }

            [Test]
            public void UsingVariableCreatesValidQueryMessage()
            {
                QueueMessage message = null;
                AttachNotificationServiceExpectation(q => { message = q; });

                var val = 4;
                _producer.Queue<INotificationService>(s => s.DeleteAllNotifications(val));

                Assert.AreEqual("DeleteAllNotifications", message.Method);
                Assert.AreEqual("4", message.SerializedArguments.Single());
            }

            [Test]
            public void UsingLogicCreatesValidQueryMessage()
            {
                QueueMessage message = null;
                AttachNotificationServiceExpectation(q => { message = q; });

                var val = 4;
                _producer.Queue<INotificationService>(s => s.DeleteAllNotifications(int.Parse(val.ToString())));

                Assert.AreEqual("DeleteAllNotifications", message.Method);
                Assert.AreEqual("4", message.SerializedArguments.Single());
            }

            [Test]
            public void MultipleArgumentsCreatesValidQueryMessage()
            {
                QueueMessage message = null;
                AttachNotificationServiceExpectation(q => { message = q; });

                _producer.Queue<INotificationService>(s => s.CreateNotification(2, "someid", "some message"));

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

                _producer.Queue<INotificationService>(s => s.CreateNotification(new ComplexType() 
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
                _producer.Queue<INotificationService>(n => n.DeleteAllNotifications(int.Parse(n.ToString())));
            }

            [Test]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ExceptionWhenBaseObjectDoesntMatch()
            {
                _producer.Queue<INotificationService>(n => _producer.ToString());
            }

            [Test]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ExceptionWhenCallingMethodWithReturnValue()
            {
                _producer.Queue<INotificationService>(n => n.FakeService());
            }

            [Test]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ServiceMustHaveQueryAttribute()
            {
                _producer.Queue<IFakeService>(s => s.SomeMethod());
            }

            private void AttachNotificationServiceExpectation(Action<QueueMessage> messageSave)
            {
                _cloudQueue.Put("Notification", Arg.Any<IObservable<QueueMessage>>()).Returns(c =>
                {
                    var obs = (IObservable<QueueMessage>)c.Args()[1];
                    obs.Subscribe(messageSave);
                    return null;
                });
            }
        }
    }
}
