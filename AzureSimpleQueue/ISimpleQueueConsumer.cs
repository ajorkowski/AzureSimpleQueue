namespace AzureSimpleQueue
{
    public interface ISimpleQueueConsumer
    {
        ISimpleQueueExecutor AttachQueuedServices(string storageAccountConnectionString, params object[] services);
    }
}
