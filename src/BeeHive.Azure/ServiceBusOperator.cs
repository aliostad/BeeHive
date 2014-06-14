using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private TimeSpan _longPollingTimeout;

        public ServiceBusOperator(string connectionString)
            : this(connectionString, TimeSpan.FromSeconds(30))
        {

        }

        public ServiceBusOperator(string connectionString, TimeSpan longPollingTimeout)
        {
            _longPollingTimeout = longPollingTimeout;
            _connectionString = connectionString;
            _namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
        }

       
        private async Task<bool> QueueExistsAsync(string name)
        {
            try
            {
                return await _namespaceManager.QueueExistsAsync(name);
            }
            catch (AggregateException e)
            {
                if (e.InnerException is MessagingException &&
                    e.InnerException.Message.StartsWith("Cannot get entity"))
                    return false;
                throw e;
            }
            catch (MessagingException ex)
            {
                if (ex.Message.StartsWith("Cannot get entity"))
                    return false;
                else
                    throw ex;

            }
            
        }

        public async Task PushAsync(Event message)
        {

            if (await QueueExistsAsync(message.QueueName))
            {
                var client = QueueClient.CreateFromConnectionString(_connectionString, message.QueueName);
                await client.SendAsync(message.ToMessage());
            }
            else
            {
                if(!await _namespaceManager.TopicExistsAsync(message.QueueName))
                    throw new InvalidOperationException("Queue or topic does not exist: " + message.QueueName);

                var client = TopicClient.CreateFromConnectionString(_connectionString, message.QueueName);
                await client.SendAsync(message.ToMessage());
            }

        }

        public async Task PushBatchAsync(IEnumerable<Event> messages)
        {
            if (!messages.Any())
                return;

            var message = messages.First();

            if (await QueueExistsAsync(message.QueueName))
            {
                var client = QueueClient.CreateFromConnectionString(_connectionString, message.QueueName);
                await client.SendBatchAsync(messages.Select(x => x.ToMessage()));
            }
            else
            {
                if (!await _namespaceManager.TopicExistsAsync(message.QueueName))
                    throw new InvalidOperationException("Queue or topic does not exist: " + message.QueueName);

                var client = TopicClient.CreateFromConnectionString(_connectionString, message.QueueName);
                await client.SendBatchAsync(messages.Select(x => x.ToMessage()));
            }
        }

   

        public async Task<PollerResult<Event>> NextAsync(QueueName name)
        {
            try
            {

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
                    message = await client.ReceiveAsync(_longPollingTimeout); 
                }

                return new PollerResult<Event>(message != null,
                        message == null
                            ? null
                            : message.ToEvent(name)
                        );
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
                return new PollerResult<Event>(false, null);
            }
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

        public Task DeferAsync(Event message, TimeSpan howLong)
        {
            var brokeredMessage = (BrokeredMessage)message.UnderlyingMessage;
            return brokeredMessage.DeferAsync() ; // TODO: use howLong
        }


        public Task CreateQueueAsync(QueueName name)
        {
            if (name.IsSimpleQueue)
                return _namespaceManager.CreateQueueAsync(name.TopicName);
            else
            {
                if (name.IsTopic)
                    return _namespaceManager.CreateTopicAsync(name.TopicName);
                else
                    return _namespaceManager.CreateSubscriptionAsync(name.TopicName,
                        name.SubscriptionName);
            }
        }

        public Task DeleteQueueAsync(QueueName name)
        {
            if (name.IsSimpleQueue)
                return _namespaceManager.DeleteQueueAsync(name.TopicName);
            else
            {
                if (name.IsTopic)
                    return _namespaceManager.DeleteTopicAsync(name.TopicName);
                else
                    return _namespaceManager.DeleteSubscriptionAsync(name.TopicName,
                        name.SubscriptionName);
            }
        }

        public Task<bool> QueueExistsAsync(QueueName name)
        {
            if (name.IsSimpleQueue)
                return _namespaceManager.QueueExistsAsync(name.TopicName);
            else
            {
                if (name.IsTopic)
                    return _namespaceManager.TopicExistsAsync(name.TopicName);
                else
                    return _namespaceManager.SubscriptionExistsAsync(name.TopicName,
                        name.SubscriptionName);
            }
        }
    }
}
