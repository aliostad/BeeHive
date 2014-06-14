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
        Task<PollerResult<T>> NextAsync(QueueName name);

        Task AbandonAsync(T message);

        Task CommitAsync(T message);

        Task DeferAsync(T message, TimeSpan howLong);
    }

    public interface ITopicOperator<T>
    {
        Task PushAsync(T message);

        Task PushBatchAsync(IEnumerable<T> messages);

    }


    public interface IQueueOperator<T> : ITopicOperator<T>, ISubscriptionOperator<T>
    {
        
        Task CreateQueueAsync(QueueName name);

        Task DeleteQueueAsync(QueueName name);

        Task<bool> QueueExistsAsync(QueueName name);

    }

    public interface IEventQueueOperator : IQueueOperator<Event>
    {
        
    }

}
