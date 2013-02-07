using Microsoft.Experience.CloudFx.Extensions.Storage;

namespace AzureSimpleQueue
{
    public interface ISimpleQueueListener
    {
        ICloudQueueListenerExtension<QueueMessage> Listener { get; }
    }
}
