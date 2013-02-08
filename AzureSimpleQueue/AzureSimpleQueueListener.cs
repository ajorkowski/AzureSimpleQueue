using System.Linq;
using System.Reactive;
using System.Reflection;
using Microsoft.Experience.CloudFx.Extensions.Storage;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AzureSimpleQueue
{
    public sealed class AzureSimpleQueueListener : ISimpleQueueListener
    {
        private readonly ICloudQueueListenerExtension<QueueMessage> _listener;
        private readonly object _service;
        private readonly Lazy<MethodInfo[]> _methods; 

        public AzureSimpleQueueListener(ICloudQueueListenerExtension<QueueMessage> listener, object service)
        {
            _listener = listener;
            _service = service;

            // Lazy loading is better??
            _methods = new Lazy<MethodInfo[]>(() => _service.GetType().GetMethods());

            var subscriber = Observer.Create<QueueMessage>(ExecuteServiceMethod);
            listener.Subscribe(subscriber);
        }

        private void ExecuteServiceMethod(QueueMessage message)
        {
            var sArgs = message.SerializedArguments == null ? new List<string>() : message.SerializedArguments.ToList();

            // Find a matching method (note we only care about the name and no of arguments when matching...)
            var possibleMethods = (from m in _methods.Value
                                   let p = m.GetParameters()
                                   where m.Name == message.Method
                                         && p.Length == sArgs.Count
                                   select new { Method = m, Parameters = p }).ToList();

            if (possibleMethods.Count > 1)
            {
                throw new InvalidOperationException("The method '" + message.Method + "' with " + sArgs.Count + " parameter(s) has more than one overload, which is not supported (even if they are different types...)");
            }

            if (possibleMethods.Count == 0)
            {
                throw new InvalidOperationException("The method '" + message.Method + "' with " + sArgs.Count + " parameter(s) does not exist on the service '" + _service.GetType().Name + "'");
            }

            // Use the method sig to deserialise the message
            var method = possibleMethods[0];
            var args = new object[sArgs.Count];

            for (int i = 0; i < sArgs.Count; i++)
            {
                try
                {
                    args[i] = JsonConvert.DeserializeObject(sArgs[i], method.Parameters[i].ParameterType);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Cannot deserialise string into type '" + method.Parameters[i].ParameterType.Name +"': " + sArgs[i], e);
                }
            }

            // Execute the method
            method.Method.Invoke(_service, args);
        }

        public ICloudQueueListenerExtension<QueueMessage> Listener
        {
            get
            {
                return _listener;
            }
        }
    }
}
