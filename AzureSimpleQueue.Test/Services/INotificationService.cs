namespace AzureSimpleQueue.Test.Services
{
    [QueuedService(Name = "Notification")]
    public interface INotificationService
    {
        void CreateNotification(long userId, string notificationId, string message);
        void DeleteAllNotifications(long userId);
    }
}
