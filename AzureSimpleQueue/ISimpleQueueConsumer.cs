using Microsoft.Experience.CloudFx.Framework.Extensibility;

namespace AzureSimpleQueue
{
    public interface ISimpleQueueConsumer
    {
        ISimpleQueueExecutor AttachQueuedServices(string storageAccountConnectionString, params object[] services);
        ISimpleQueueExecutor AttachQueuedServices(string storageAccountConnectionString, IExtensibleComponent component, params object[] services);
    }
}
