using System;

namespace AzureSimpleQueue
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public class QueuedServiceAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
