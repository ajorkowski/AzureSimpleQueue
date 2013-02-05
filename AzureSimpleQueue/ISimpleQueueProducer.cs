using System;
using System.Linq.Expressions;

namespace AzureSimpleQueue
{
    public interface ISimpleQueueProducer
    {
        void Queue<T>(Expression<Action<T>> serviceAction);
    }
}
