using System.Collections.Generic;

namespace AzureSimpleQueue
{
    public class QueryMessage
    {
        public string Method { get; set; }
        public IEnumerable<string> SerializedArguments { get; set; } 
    }
}
