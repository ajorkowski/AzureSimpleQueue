using System.Collections.Generic;

namespace AzureSimpleQueue
{
    public class QueueMessage
    {
        public string Method { get; set; }
        public IEnumerable<string> SerializedArguments { get; set; } 
    }
}
