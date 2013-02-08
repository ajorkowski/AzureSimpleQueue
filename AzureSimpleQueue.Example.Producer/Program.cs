using System;
using System.ServiceModel;
using Autofac;
using Microsoft.Experience.CloudFx.Extensions.Storage;
using Microsoft.Experience.CloudFx.Framework.Extensibility;
using Microsoft.Experience.CloudFx.Extensions.Configuration;
using System.Reactive.Linq;
using AzureSimpleQueue.Example.Shared;
using Microsoft.Experience.CloudFx.Framework.Storage;

namespace AzureSimpleQueue.Example.Producer
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Setup cloudfx (see app.config for retry and queue settings)
            var cloudfx = new SimpleExtensibilityContainer();
            cloudfx.Extensions.EnsureExists<ConfigurationProviderExtension>();
            cloudfx.Extensions.EnsureExists<CloudStorageProviderExtension>(); 

            // Register components in IoC container
            var builder = new ContainerBuilder();
            builder.Register(c => cloudfx.Extensions.Find<ICloudStorageProviderExtension>().DefaultQueueStorage).As<ICloudQueueStorage>().SingleInstance();
            builder.RegisterGeneric(typeof (AzureSimpleQueue<>)).As(typeof (ISimpleQueue<>)).SingleInstance();
            var container = builder.Build();

            // Get the queue that we want to post to...
            var exampleQueue = container.Resolve<ISimpleQueue<IExampleQueue>>();

            // Start sending messages every second
            var tick = Observable.Interval(new TimeSpan(0, 0, 1));
            tick.Subscribe(time =>
            {
                string message = "The time tick is: " + time;
                Console.WriteLine("Sending message: " + message);
                exampleQueue.Queue(q => q.SimpleMessage(message));
            });

            // Press a key to stop...
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();
        }
    }

    public class SimpleExtensibilityContainer : IExtensibleComponent
    {
        public SimpleExtensibilityContainer()
        {
            Extensions = new ExtensibleComponentCollection<IExtensibleComponent>(this);
        }

        public IExtensionCollection<IExtensibleComponent> Extensions { get; private set; }
    }
}
