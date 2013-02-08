# AzureSimpleQueue

A simple queue wrapper so that you can call queue services with easy to use functional expressions. 
Powered by the CloudFX library.

## Get it

Install-Package AzureSimpleQueue

## Usage

CloudFx is doing all the queue handling (such as retry policies etc). AzureSimpleQueue is designed
to sit on top of that to make it clean and easy.

### Queued Services

Create the service that will be called using a queue. You have to create an interface
marked with the `QueuedService` attribute.

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

// On the comsumer side, create the implementation of your service (the IQueuedService is useful for DI)
public class NotificationService : INotificationService
{
    // remember calls to your functions are the result of messages
    // They can be called in any order and perhaps multiple times
    // Try to make your functions indempotent
	void CreateNotification(long userId, string notificationId, string message) { ... }
    void DeleteAllNotifications(long userId) { ... }
}
```

### Producer

Use the `AzureSimpleQueue` type to create the queue reference

```
var queue = new AzureSimpleQueue<INotificationService>(/*pass in ICloudQueueStorage from cloudFx*/);
```

Now you can queue messages using lambda expressions

```
queue.Queue(s => s.DeleteAllNotifications(userId));
```

Note that parameters of the method in the expression are *EXECUTED* and the result serialised. 
This means you can have complex logic with external variables etc in your expression in the parameters

### Consumer

On the consumer side all you need to do is to link up your service with the queue it belongs to.
The first step in this process is to create the `AzureSimpleQueueConsumer`:

```
var consumer = new AzureSimpleQueueConsumer(/*pass in IExtensibleComponent from cloudFx*/);
```

From this you can attach as many services you need and then all you need to do is to start them

```
var services = consumer.AttachQueuedServices(queueAccountConnectionString, new NotificationService(), ...);
services.Start();
```

Now the queue related to the services will be watched and the appropriate methods executed when a
message is received.

## IoC support

There are matching interfaces that are useful for IoC, code below is from the code examples 
that use Autofac:

### Producer
```
var builder = new ContainerBuilder();
builder.Register(c => cloudfx.Extensions.Find<ICloudStorageProviderExtension>().DefaultQueueStorage).As<ICloudQueueStorage>().SingleInstance();
builder.RegisterGeneric(typeof (AzureSimpleQueue<>)).As(typeof (ISimpleQueue<>)).SingleInstance();
var container = builder.Build();

// Get the queue that we want to post to...
var exampleQueue = container.Resolve<ISimpleQueue<IExampleQueue>>();

exampleQueue.Queue(q => q.SimpleMessage("Some message"));
```

### Consumer
```
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
```

## Examples

To run the examples you need to have the azure storage emulator running locally.

## Limitations

### JSON.NET Serialization

All serialization is done with JSON.NET so any parameters must be serialisable/deserialisable
using it.

### Method overloading

You cannot have two methods on the same service with the same number of arguments. 
For instance trying to message either one of the following methods will not work:

```
void SomeServiceMethod(int id);
void SomeServiceMethod(string other);
```

## License
MIT Licensed