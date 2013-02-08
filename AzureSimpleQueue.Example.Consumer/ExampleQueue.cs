using AzureSimpleQueue.Example.Shared;
using System;

namespace AzureSimpleQueue.Example.Consumer
{
    public class ExampleQueue : IExampleQueue
    {
        public void SimpleMessage(string message)
        {
            Console.WriteLine("Recieved Message: " + message);
        }
    }
}
