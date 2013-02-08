using System;
using System.ServiceModel;
using AzureSimpleQueue.Example.Shared;
using Microsoft.Experience.CloudFx.Extensions.Configuration;
using Microsoft.Experience.CloudFx.Extensions.Storage;
using Microsoft.Experience.CloudFx.Framework.Configuration;
using Microsoft.Experience.CloudFx.Framework.Extensibility;
using Autofac;

namespace AzureSimpleQueue.Example.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Setup cloudfx (see app.config for retry and queue settings)
            var cloudfx = new SimpleExtensibilityContainer();
            cloudfx.Extensions.EnsureExists<ConfigurationProviderExtension>();
            cloudfx.Extensions.EnsureExists<CloudStorageProviderExtension>();

            // Register components in IoC container
            var builder = new ContainerBuilder();
            builder.RegisterInstance<IExtensibleComponent>(cloudfx).SingleInstance();
            builder.RegisterType<ExampleQueue>().As<IExampleQueue>().SingleInstance();
            builder.RegisterType<AzureSimpleQueueConsumer>().As<ISimpleQueueConsumer>().SingleInstance();
            var container = builder.Build();

            // Get the consumer
            var consumer = container.Resolve<ISimpleQueueConsumer>();

            // Register the services
            string queueAccount = CloudApplicationConfiguration.Current.GetSection<StorageAccountConfigurationSection>(StorageAccountConfigurationSection.SectionName).DefaultQueueStorage;
            var services = consumer.AttachQueuedServices(queueAccount, container.Resolve<IExampleQueue>());

            // Start them up
            services.Start();

            Console.WriteLine("Press any key to stop consuming...");
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
