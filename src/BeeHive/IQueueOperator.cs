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

        Task AbandonAsync(T message);

        Task CommitAsync(T message);
    }

    public interface ITopicOperator<T>
    {
        Task PushAsync(T message);
    }


    public interface IQueueOperator<T> : ITopicOperator<T>, ISubscriptionOperator<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="subscriptions">If this is empty, then it is a simple queue</param>
        /// <returns></returns>
        Task CreateQueueAsync(string topicName, params string[] subscriptions);

        Task DeleteQueueAsync(string topicName);

        Task AddSubscriptionAsync(string topicName, string subscriptionName);

        Task RemoveSubscriptionAsync(string topicName, string subscriptionName);

        Task SetupQueueAsync(QueueName name);

    }

    public interface IEventQueueOperator : IQueueOperator<Event>
    {
        
    }

}
