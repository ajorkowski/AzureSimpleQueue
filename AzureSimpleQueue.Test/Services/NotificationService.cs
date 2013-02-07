namespace AzureSimpleQueue.Test.Services
{
    public class NotificationService : INotificationService
    {
        // Create structure like this for easy testing (can mock the injected service)
        private readonly INotificationService _service;

        public NotificationService(INotificationService service)
        {
            _service = service;
        }

        public void CreateNotification(long userId, string notificationId, string message)
        {
            _service.CreateNotification(userId, notificationId, message);
        }

        public void CreateNotification(ComplexType notificationDetails)
        {
            _service.CreateNotification(notificationDetails);
        }

        public void DeleteAllNotifications(long userId)
        {
            _service.DeleteAllNotifications(userId);
        }

        public string FakeService()
        {
            return _service.FakeService();
        }
    }
}
