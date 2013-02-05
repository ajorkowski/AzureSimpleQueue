using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Experience.CloudFx.Framework.Storage;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace AzureSimpleQueue
{
    public class AzureSimpleQueueProducer : ISimpleQueueProducer
    {
        private readonly ICloudQueueStorage _queue;

        public AzureSimpleQueueProducer(ICloudQueueStorage queue)
        {
            _queue = queue;
        }

        public void Queue<T>(Expression<Action<T>> serviceAction)
        {
            // Is this type actually a service
            var type = typeof (T);
            var attributes = type.GetCustomAttributes(typeof (QueuedServiceAttribute), true);
            if(!attributes.Any())
            {
                throw new InvalidOperationException("A Queued service must have the [QueuedService] attribute");
            }

            // Get the name of the queue
            var attribute = attributes[0] as QueuedServiceAttribute;
            var name = attribute.Name ?? type.Name;

            // convert expression to message
            var message = FindMessage(serviceAction);

            // Send it off
            _queue.Put(name, Observable.Return(message));
        }

        private QueryMessage FindMessage<T>(Expression<Action<T>> serviceAction)
        {
            var method = serviceAction.Body as MethodCallExpression;
            if(method == null)
            {
                throw new InvalidOperationException("Expecting a method call expression");
            }

            if(method.Object == null || method.Object.Type != typeof(T))
            {
                throw new InvalidOperationException("Expecting method to be called on the service");
            }

            if(method.Method.ReturnType != typeof(void))
            {
                throw new InvalidOperationException("Expecting only void methods to be used - one way messaging only");
            }

            var arguments = new List<string>();
            int count = 0;
            foreach (var arg in method.Arguments)
            {
                var constant = arg as ConstantExpression;
                if(constant == null)
                {
                    throw new InvalidOperationException("Expecting constant but didnt get one on arg[" + count + "]: " + arg);
                }

                // Serialise using JSON.NET
                string value = JsonConvert.SerializeObject(constant.Value);

                // Push arg
                arguments.Add(value);

                count++;
            }

            return new QueryMessage
            {
                Method = method.Method.Name,
                SerializedArguments = arguments
            };
        }
    }
}
