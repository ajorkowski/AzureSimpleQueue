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

        private QueueMessage FindMessage<T>(Expression<Action<T>> serviceAction)
        {
            var method = serviceAction.Body as MethodCallExpression;
            if(method == null)
            {
                throw new InvalidOperationException("Expecting a method call expression");
            }

            if(method.Object == null || method.Object.Type != typeof(T))
            {
                throw new InvalidOperationException("Expecting base object to be the queued service");
            }

            if(method.Method.ReturnType != typeof(void))
            {
                throw new InvalidOperationException("Expecting only void methods to be used - one way messaging only");
            }

            var arguments = new List<string>();
            int count = 0;
            foreach (var arg in method.Arguments)
            {
                string value;

                // If it is a constant type just grab the value (much quicker than alternative)
                var constant = arg as ConstantExpression;
                if(constant != null)
                {
                    // JSON serialise the value
                    value = JsonConvert.SerializeObject(constant.Value);
                }
                else
                {
                    try
                    {
                        // We are going to execute the expression to get the value... this will cover any complex logic in this arg
                        // We do assume that there are no arguments in this expression - which makes sense as the only arg could
                        // be the service... and that makes no sense at all...
                        var lambda = Expression.Lambda(arg);

                        // Compile so that we can execute it!
                        var func = lambda.Compile();

                        // Execute the function so that we can get the 'real value'
                        var result = func.DynamicInvoke();

                        // JSON serialise the value
                        value = JsonConvert.SerializeObject(result);
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException("The argument could not be executed to find it's value - most likely you have used the lambda parameter in the arg", e);
                    }
                } 

                // Push arg
                arguments.Add(value);

                count++;
            }

            return new QueueMessage
            {
                Method = method.Method.Name,
                SerializedArguments = arguments
            };
        }
    }
}
