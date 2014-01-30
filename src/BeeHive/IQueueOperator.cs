using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Scheduling;

namespace BeeHive
{

    public interface ISubscriptionOperator<T>
    {
        Task<PollerResult<T>> NextAsync(string queueName);

        Task Abandon(T message);

        Task Commit(T message);
    }

    public interface ITopicOperator<T>
    {
        Task PushAsync(T message);
    }


    public interface IQueueOperator<T> : ITopicOperator<T>, ISubscriptionOperator<T>
    {
        
    }

    public interface IEventQueueOperator : IQueueOperator<Event>
    {
        
    }

}
