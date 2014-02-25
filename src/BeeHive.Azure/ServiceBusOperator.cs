using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Scheduling;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace BeeHive.Azure
{
    public class ServiceBusOperator : IEventQueueOperator
    {
        private string _connectionString;
        private NamespaceManager _namespaceManager;


        public ServiceBusOperator(string connectionString)
        {
            _connectionString = connectionString;
            _namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
        }

        public async Task PushAsync(Event message)
        {

            if (await _namespaceManager.QueueExistsAsync(message.QueueName))
            {
                var client = QueueClient.CreateFromConnectionString(_connectionString, message.QueueName);
                await client.SendAsync(message.ToMessage());
            }
            else
            {
                if(!await _namespaceManager.TopicExistsAsync(message.QueueName))
                    throw new InvalidOperationException("Queue or topic does not exist.");

                var client = TopicClient.CreateFromConnectionString(_connectionString, message.QueueName);
                await client.SendAsync(message.ToMessage());
            }

        }

        public async Task PushBatchAsync(IEnumerable<Event> messages)
        {
            if (!messages.Any())
                return;

            var message = messages.First();

            if (await _namespaceManager.QueueExistsAsync(message.QueueName))
            {
                var client = QueueClient.CreateFromConnectionString(_connectionString, message.QueueName);
                await client.SendBatchAsync(messages.Select(x => x.ToMessage()));
            }
            else
            {
                if (!await _namespaceManager.TopicExistsAsync(message.QueueName))
                    throw new InvalidOperationException("Queue or topic does not exist.");

                var client = TopicClient.CreateFromConnectionString(_connectionString, message.QueueName);
                await client.SendBatchAsync(messages.Select(x => x.ToMessage()));
            }
        }

        public async Task<PollerResult<Event>> NextAsync(string queueName)
        {
            var name = new QueueName(queueName);
            BrokeredMessage message = null;
            if (name.IsSimpleQueue)
            {
                var client = QueueClient.CreateFromConnectionString(_connectionString, name.TopicName);
                message = await client.ReceiveAsync();
                
            }
            else
            {
                var client = SubscriptionClient.CreateFromConnectionString(_connectionString, 
                    name.TopicName, name.SubscriptionName);
                message = await client.ReceiveAsync(TimeSpan.FromSeconds(30)); // TODO: replace with param
            }

            return new PollerResult<Event>(message != null,
                    message == null
                        ? null
                        : message.ToEvent(name)
                    );
        }

        public Task AbandonAsync(Event message)
        {
            var brokeredMessage = (BrokeredMessage) message.UnderlyingMessage;
            return brokeredMessage.AbandonAsync();
        }

        public Task CommitAsync(Event message)
        {
            var brokeredMessage = (BrokeredMessage)message.UnderlyingMessage;
            return brokeredMessage.CompleteAsync();
        }

        public async Task CreateQueueAsync(string topicName, params string[] subscriptions)
        {
            if (subscriptions.Length == 0)
            {
                if (!await _namespaceManager.QueueExistsAsync(topicName))
                    await _namespaceManager.CreateQueueAsync(topicName);
            }
            else
            {
                if(! await _namespaceManager.TopicExistsAsync(topicName))
                    await _namespaceManager.CreateTopicAsync(topicName);

                foreach (var subscription in subscriptions)
                {
                    if(! await _namespaceManager.SubscriptionExistsAsync(topicName, subscription))
                        await _namespaceManager.CreateSubscriptionAsync(topicName, subscription);
                }
            }
        }

        public async Task DeleteQueueAsync(string topicName)
        {
            if (await _namespaceManager.QueueExistsAsync(topicName))
            {
                await _namespaceManager.DeleteQueueAsync(topicName);
            }
            else
            {
                if (await _namespaceManager.TopicExistsAsync(topicName))
                    await _namespaceManager.DeleteTopicAsync(topicName);
            }

        }

        public Task AddSubscriptionAsync(string topicName, string subscriptionName)
        {
            return _namespaceManager.CreateSubscriptionAsync(topicName, subscriptionName); 
        }

        public Task RemoveSubscriptionAsync(string topicName, string subscriptionName)
        {
            return _namespaceManager.DeleteSubscriptionAsync(topicName, subscriptionName);
        }

        public Task SetupQueueAsync(QueueName name)
        {
            if (name.IsSimpleQueue)
            {
               
                return this.CreateQueueAsync(name.TopicName);
            }
            else
            {
                return this.CreateQueueAsync(name.TopicName, name.SubscriptionName);
            }
        }

        
    }
}
