using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSimpleQueue
{
    public interface ISimpleQueueConsumer
    {
        ISimpleQueueExecutor StartQueuedServices(params object[] services);
    }
}
