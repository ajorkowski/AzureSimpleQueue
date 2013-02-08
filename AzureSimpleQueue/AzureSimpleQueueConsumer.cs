using System;
using Microsoft.Experience.CloudFx.Extensions.Storage;
using System.Linq;
using Microsoft.Experience.CloudFx.Framework.Extensibility;
using Microsoft.Experience.CloudFx.Framework.Storage;

namespace AzureSimpleQueue
{
    public sealed class AzureSimpleQueueConsumer : ISimpleQueueConsumer
    {
        private readonly IExtensibleComponent _component;

        public AzureSimpleQueueConsumer(IExtensibleComponent component)
        {
            _component = component;
        }

        public ISimpleQueueExecutor AttachQueuedServices(string storageAccountConnectionString, params object[] services)
        {
            if(services == null || !services.Any())
            {
                throw new InvalidOperationException("You must specify services to attach listeners to!");
            }

            var listeners = services.Select(s => AttachService(storageAccountConnectionString, s)).ToList();
            return new AzureSimpleQueueExecutor(listeners);
        }

        private ISimpleQueueListener AttachService(string storageAccountConnectionString, object service)
        {
            // Search interfaces for 'QueuedServiceAttribute' def and get name
            string name = (from interfaceType in service.GetType().GetInterfaces() 
                           let attr = interfaceType.GetCustomAttributes(typeof (QueuedServiceAttribute), false) 
                           where attr.Any() 
                           select ((QueuedServiceAttribute) attr[0]).Name ?? interfaceType.Name).FirstOrDefault();

            if(name == null)
            {
                throw new InvalidOperationException("Service type '" + service.GetType().Name + "' must inherit from an interface marked with the [QueuedService] attribute");
            }

            var location = new CloudQueueLocation { QueueName = name, StorageAccount = storageAccountConnectionString };
            var listener = new CloudQueueListenerExtension<QueueMessage>(location, _component);
            return new AzureSimpleQueueListener(listener, service);
        }
    }
}
