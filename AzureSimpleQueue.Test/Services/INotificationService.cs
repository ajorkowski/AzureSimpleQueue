namespace AzureSimpleQueue.Test.Services
{
    public class ComplexType
    {
        public int UserId { get; set; }
        public string NotificationId { get; set; }
        public string Message { get; set; }
    }

    [QueuedService(Name = "Notification")]
    public interface INotificationService
    {
        void CreateNotification(long userId, string notificationId, string message);
        void CreateNotification(ComplexType notificationDetails);

        void DeleteAllNotifications(long userId);

        string FakeService();

        // Need this to test overridden methods
        void MethodThatDoesntWork(int id);
        void MethodThatDoesntWork(string id);
    }
}
