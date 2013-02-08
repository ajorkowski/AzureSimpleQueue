namespace AzureSimpleQueue.Example.Shared
{
    [QueuedService(Name = "AzureSimpleQueueExampleQueue")]
    public interface IExampleQueue
    {
        void SimpleMessage(string message);
    }
}
