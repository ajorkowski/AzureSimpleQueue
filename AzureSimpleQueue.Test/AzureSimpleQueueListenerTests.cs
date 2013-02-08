using System.Collections.Generic;
using AzureSimpleQueue.Test.Services;
using Microsoft.Experience.CloudFx.Extensions.Storage;
using NSubstitute;
using NUnit.Framework;
using System;

namespace AzureSimpleQueue.Test
{
    [TestFixture]
    public class AzureSimpleQueueListenerTests
    {
        private IObserver<QueueMessage> _observer;
        private INotificationService _service;
        private AzureSimpleQueueListener _notification;
            
        [SetUp]
        public void Init()
        {
            _observer = null;
            var listener = Substitute.For<ICloudQueueListenerExtension<QueueMessage>>();
            listener.Subscribe(null).ReturnsForAnyArgs(c => { _observer = (IObserver<QueueMessage>)c.Args()[0]; return null; });

            _service = Substitute.For<INotificationService>();
            _notification = new AzureSimpleQueueListener(listener, _service);
        }

        [Test]
        public void MethodWithMultipleParametersConnects()
        {
            _observer.OnNext(new QueueMessage
            {
                Method = "CreateNotification",
                SerializedArguments = new [] {"2", "\"4\"", "\"some message\""}
            });

            _service.Received(1).CreateNotification(2, "4", "some message");
        }

        [Test]
        public void MethodWithComplexParameterConnects()
        {
            _observer.OnNext(new QueueMessage
            {
                Method = "CreateNotification",
                SerializedArguments = new[] { "{ UserId: 2, NotificationId: '4', Message:'some message' }" }
            });

            _service.Received(1).CreateNotification(Arg.Is<ComplexType>(t => t.UserId == 2 && t.NotificationId == "4" && t.Message == "some message"));
        }

        [Test]
        public void MethodWithNoParametersConnects()
        {
            _observer.OnNext(new QueueMessage { Method = "FakeService" });

            _service.Received(1).FakeService();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExceptionWhenTryToExecuteRandomMessage()
        {
            _observer.OnNext(new QueueMessage { Method = "RandomMethodThatDoesntExist" });
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExceptionWhenTryToSendOverloadedMethods()
        {
            _observer.OnNext(new QueueMessage { Method = "MethodThatDoesntWork", SerializedArguments = new List<string> { "4" }});
        }
    }
}
