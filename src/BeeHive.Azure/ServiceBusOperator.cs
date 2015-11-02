using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BeeHive.Scheduling;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace BeeHive.Azure
{
    public class ServiceBusOperator : IEventQueueOperator
    {
        private NamespaceManager _namespaceManager;
        private TimeSpan _longPollingTimeout;
        private ClientProvider _clientProvider;
        public ServiceBusOperator(string connectionString)
            : this(connectionString, 
            TimeSpan.FromSeconds(30))
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="longPollingTimeout"></param>
        /// Turn it on if you know the limit will not be reached</param>
        public ServiceBusOperator(string connectionString, 
            TimeSpan longPollingTimeout)
        {
            _longPollingTimeout = longPollingTimeout;
            _namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            _clientProvider = new ClientProvider(connectionString, true);
        }

        public async Task PushAsync(Event message)
        {
            var queueName = new QueueName(message.QueueName);
            if (queueName.IsSimpleQueue)
            {
                var client = _clientProvider.GetQueueClient(queueName);
                await client.SendAsync(message.ToMessage());
            }
            else
            {
                var client = _clientProvider.GetTopicClient(queueName);
                await client.SendAsync(message.ToMessage());
            }
        }

        public async Task PushBatchAsync(IEnumerable<Event> messages)
        {

            const int BatchSize = 50;
            
            var msgs = messages.ToArray();
            if (!msgs.Any())
                return;

            var message = msgs.First();
            var queueName = new QueueName(message.QueueName);
            int i = 0;

            if (queueName.IsSimpleQueue)
            {
                var client = _clientProvider.GetQueueClient(queueName);
                while (i < msgs.Length)
                {
                    await client.SendBatchAsync(msgs.Skip(i).Take(BatchSize).Select(x => x.ToMessage()));
                    i += BatchSize;
                }
            }
            else
            {
                var client = _clientProvider.GetTopicClient(queueName);
                while (i < msgs.Length)
                {
                    await client.SendBatchAsync(msgs.Skip(i).Take(BatchSize).Select(x => x.ToMessage()));
                    i += BatchSize;
                }
            }
        }

        public async Task<PollerResult<Event>> NextAsync(QueueName name)
        {
            try
            {

                BrokeredMessage message = null;
                if (name.IsSimpleQueue)
                {
                    var client = _clientProvider.GetQueueClient(name);
                    message = await client.ReceiveAsync(_longPollingTimeout);
                }
                else
                {
                    var client = _clientProvider.GetSubscriptionClient(name);
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
                TheTrace.TraceWarning(e.ToString());
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
            brokeredMessage.ScheduledEnqueueTimeUtc = DateTime.UtcNow.Add(howLong);
            return PushAsync(message);
        }

        public async Task KeepExtendingLeaseAsync(Event message, TimeSpan howLong, CancellationToken cancellationToken)
        {

            await Task.Delay(new TimeSpan(2 * howLong.Ticks / 3), cancellationToken);

            while (true)
            {
                try
                {
                    if(cancellationToken.IsCancellationRequested)
                        break;

                    var underlyingMessage = (BrokeredMessage) message.UnderlyingMessage;
                    await underlyingMessage.RenewLockAsync();
                    await Task.Delay(new TimeSpan(2*howLong.Ticks/3), cancellationToken);
                }
                catch (Exception exception)
                {
                    TheTrace.TraceError(exception.ToString());
                    break;
                }
            }
        }


        public async Task CreateQueueAsync(QueueName name)
        {
            if (name.IsSimpleQueue)
                await _namespaceManager.CreateQueueAsync(name.TopicName);
            else
            {
                if (name.IsTopic)
                    await _namespaceManager.CreateTopicAsync(name.TopicName);
                else
                {
                    await this.SetupQueueAsync(QueueName.FromTopicName(name.TopicName));
                    await _namespaceManager.CreateSubscriptionAsync(name.TopicName,
                        name.SubscriptionName);
                }
                    
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

        public async Task<bool> QueueExistsAsync(QueueName name)
        {
            if (name.IsSimpleQueue)
                return await _namespaceManager.QueueExistsAsync(name.TopicName);
            else
            {
                if (name.IsTopic)
                    return await _namespaceManager.TopicExistsAsync(name.TopicName);
                else
                    return
                        await _namespaceManager.TopicExistsAsync(name.TopicName) &&
                        await _namespaceManager.SubscriptionExistsAsync(name.TopicName,
                        name.SubscriptionName);
            }
        }

        internal class ClientProvider
        {

            /// <summary>
            /// All this lazy stuff is to get around issue reported by Ayende
            /// with concurrent collections
            /// </summary>
            ConcurrentDictionary<string, Lazy<object>> _cachedClients =
                new ConcurrentDictionary<string, Lazy<object>>();


            private bool _cache;
            private string _connectionString;


            public ClientProvider(string connectionString, bool cache)
            {
                _connectionString = connectionString;
                _cache = cache;
            }

            public SubscriptionClient GetSubscriptionClient(QueueName queueName)
            {

                Func<object> factory = () =>
                    SubscriptionClient.CreateFromConnectionString(_connectionString,
                        queueName.TopicName,
                        queueName.SubscriptionName);

                if (_cache)
                {
                     
                    var lazy =
                    _cachedClients.GetOrAdd(
                        queueName.ToString(),
                        (name) =>

                            new Lazy<object>(factory));

                    return (SubscriptionClient) lazy.Value;

                }

                return (SubscriptionClient) factory();
            }

            public TopicClient GetTopicClient(QueueName queueName)
            {

                Func<object> factory = () =>
                    TopicClient.CreateFromConnectionString(_connectionString,
                        queueName.TopicName);

                if (_cache)
                {

                    var lazy =
                    _cachedClients.GetOrAdd(
                        queueName.ToString(),
                        (name) =>

                            new Lazy<object>(factory));

                    return (TopicClient)lazy.Value;

                }

                return (TopicClient)factory();
            }

            public QueueClient GetQueueClient(QueueName queueName)
            {

                Func<object> factory = () =>
                    QueueClient.CreateFromConnectionString(_connectionString,
                        queueName.TopicName);

                if (_cache)
                {

                    var lazy =
                    _cachedClients.GetOrAdd(
                        queueName.ToString(),
                        (name) =>

                            new Lazy<object>(factory));

                    return (QueueClient)lazy.Value;

                }

                return (QueueClient)factory();
            }

        }
    }
}
