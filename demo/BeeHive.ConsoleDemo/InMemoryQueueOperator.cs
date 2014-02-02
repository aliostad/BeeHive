using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Scheduling;

namespace BeeHive.ConsoleDemo
{
    public class InMemoryQueueOperator : IEventQueueOperator
    {


        public Task<PollerResult<Event>> NextAsync(string queueName)
        {
            throw new NotImplementedException();
        }

        public Task PushAsync(Event message)
        {
            throw new NotImplementedException();
        }

        public Task Abandon(Event message)
        {
            throw new NotImplementedException();
        }

        public Task Commit(Event message)
        {
            throw new NotImplementedException();
        }

        public Task CreateQueue(string topicName, params string[] subscriptions)
        {
            throw new NotImplementedException();
        }

        public Task DeleteQueue(string topicName)
        {
            throw new NotImplementedException();
        }

        public Task AddSubscription(string topicName, string subscriptionName)
        {
            throw new NotImplementedException();
        }

        public Task RemoveSubscription(string topicName, string subscriptionName)
        {
            throw new NotImplementedException();
        }
    }
}
