# AzureSimpleQueue

A simple queue wrapper so that you can call queue services with easy to use functional expressions. 
Powered by the CloudFX library.

## Get it

Install-Package AzureSimpleQueue

## Usage

```
// Create a service contract that you want to run through a queue
// The name will be the name of the queue
// NOTE: All methods must be void - one way messaging supported only
[QueuedService(Name = "Notification")]
public interface INotificationService 
{
    void CreateNotification(long userId, string notificationId, string message);
    void DeleteAllNotifications(long userId);
}

// Initialise the AzureSimpleQueueProducer, you can put it into DI using ISimpleQueueProducer

// Execute the service when required
ISimpleQueueProducer producer = (... resolve producer here ...);
producer.Queue<INotificationService>(s => s.CreateNotification(1, "id", "New Notification!"));

// On the comsumer side, create the implementation of your service (the IQueuedService is useful for DI)
public class NotificationService : INotificationService, IQueuedService
{
    // remember calls to your functions are the result of messages
    // They can be called in any order and perhaps multiple times
    // Try to make your functions indempotent
}

// Initialise the AzureSimpleQueueConsumer, you can put into DI using ISimpleQueueConsumer

// Call the start method to start listening for messages
ISimpleQueueConsumer consumer = (... resolve consumer here ...);
consumer.Start();

```

## Tests

To run the tests you need to have the azure storage emulator running locally.

## License
MIT Licensed