using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Experience.CloudFx.Framework.Storage;
using Newtonsoft.Json;

namespace AzureSimpleQueue
{
    public class AzureSimpleQueue<T> : ISimpleQueue<T>
    {
        private readonly ICloudQueueStorage _queue;
        private readonly string _name;

        public AzureSimpleQueue(ICloudQueueStorage queue)
        {
            _queue = queue;

            // Is this type actually a service
            var type = typeof (T);
            var attributes = type.GetCustomAttributes(typeof (QueuedServiceAttribute), true);
            if(!attributes.Any())
            {
                throw new InvalidOperationException("A Queued service must have the [QueuedService] attribute");
            }

            // Get the name of the queue
            var attribute = attributes[0] as QueuedServiceAttribute;
            _name = attribute.Name ?? type.Name;
        }

        public void Queue(Expression<Action<T>> serviceAction)
        {
            // convert expression to message
            var message = FindMessage(serviceAction);

            // Send it off
            _queue.Put(_name, message);
        }

        private QueueMessage FindMessage(Expression<Action<T>> serviceAction)
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
