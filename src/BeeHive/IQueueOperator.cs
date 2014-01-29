using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Scheduling;

namespace BeeHive
{
    public interface IQueueOperator<T>
    {
        Task<PollerResult<T>> NextAsync(string queueName);

        Task PushAsync(T message);

        Task Abandon(T message);

        Task Commit(T message);

    }

    public interface IEventQueueOperator : IQueueOperator<Event>
    {
        
    }

}
