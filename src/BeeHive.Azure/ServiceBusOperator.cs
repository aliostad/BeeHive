using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BeeHive.Scheduling;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;


namespace BeeHive.Azure
{
    public class ServiceBusOperator : IEventQueueOperator
    {
        private ClientProvider _clientProvider;
        private string _connectionString;
        private Lazy<ManagementClient> _lazyNamespaceManager;
        private TimeSpan _maxAutoRenew;

        public bool IsEventDriven => true;

        public ServiceBusOperator(string connectionString) :
            this(connectionString, TimeSpan.FromMinutes(30))
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// Turn it on if you know the limit will not be reached</param>
        public ServiceBusOperator(string connectionString, TimeSpan maxAutoRenew)
        {
            _connectionString = connectionString;
            _clientProvider = new ClientProvider(connectionString, true);

            // NOTE: reason for lazy is not in all cases connection string allows management. 
            // Some might decide to create Queues, Topics and Subscriptions separately and give read/write access only.
            _lazyNamespaceManager = new Lazy<ManagementClient>(() => new ManagementClient(_connectionString));
            _maxAutoRenew = maxAutoRenew;
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
            var msgs = messages.ToArray();
            if (!msgs.Any())
                return;

            var message = msgs.First();
            var queueName = new QueueName(message.QueueName);
            int i = 0;

            if (queueName.IsSimpleQueue)
            {
                var client = _clientProvider.GetQueueClient(queueName);
                foreach (var batch in BatchUp(msgs))
                {
                    await client.SendAsync(batch);
                }                
            }
            else
            {
                var client = _clientProvider.GetTopicClient(queueName);
                foreach (var batch in BatchUp(msgs))
                {
                    await client.SendAsync(batch);
                }                   
            }
        }

        internal static List<List<Message>> BatchUp(IEnumerable<Event> messages)
        {
            const long SizeLimit = 240*1024; // 240KB
            var listOfBatches = new List<List<Message>>();
            var list = new List<Message>();
            var size = 0L;
            foreach (var message in messages)
            {
                var brokeredMessage = message.ToMessage();
                if (size + brokeredMessage.Size >= SizeLimit)
                {
                    listOfBatches.Add(list);
                    list = new List<Message>();
                    size = 0;
                }
                
                size += brokeredMessage.Size;
                list.Add(brokeredMessage);
            }

            listOfBatches.Add(list);
            return listOfBatches;
        }

        public async Task<PollerResult<Event>> NextAsync(QueueName name)
        {
            throw new NotSupportedException();
        }

        public Task AbandonAsync(Event message)
        {
            var brokeredMessage = (Message) message.UnderlyingMessage;
            var q = new QueueName(message.QueueName);
            if (q.IsSimpleQueue)
            {
                return _clientProvider.GetQueueClient(q).AbandonAsync(brokeredMessage.SystemProperties.LockToken);
            }
            else
            {
                return _clientProvider.GetSubscriptionClient(q).AbandonAsync(brokeredMessage.SystemProperties.LockToken);
            }            
        }

        public Task CommitAsync(Event message)
        {
            var brokeredMessage = (Message)message.UnderlyingMessage;
            var q = new QueueName(message.QueueName);
            if (q.IsSimpleQueue)
            {
                return _clientProvider.GetQueueClient(q).CompleteAsync(brokeredMessage.SystemProperties.LockToken);
            }
            else
            {
                return _clientProvider.GetSubscriptionClient(q).CompleteAsync(brokeredMessage.SystemProperties.LockToken);
            }
        }

        public Task DeferAsync(Event message, TimeSpan howLong)
        {
            var brokeredMessage = (Message)message.UnderlyingMessage;
            brokeredMessage.ScheduledEnqueueTimeUtc = DateTime.UtcNow.Add(howLong);
            return PushAsync(message);
        }

        public async Task CreateQueueAsync(QueueName name)
        {
            if (name.IsSimpleQueue)
                await _lazyNamespaceManager.Value.CreateQueueAsync(name.TopicName);
            else
            {
                if (name.IsTopic)
                    await _lazyNamespaceManager.Value.CreateTopicAsync(name.TopicName);
                else
                {
                    await this.SetupQueueAsync(QueueName.FromTopicName(name.TopicName));
                    await _lazyNamespaceManager.Value.CreateSubscriptionAsync(name.TopicName,
                        name.SubscriptionName);
                }
            }
        }

        public Task DeleteQueueAsync(QueueName name)
        {
            if (name.IsSimpleQueue)
                return _lazyNamespaceManager.Value.DeleteQueueAsync(name.TopicName);
            else
            {
                if (name.IsTopic)
                    return _lazyNamespaceManager.Value.DeleteTopicAsync(name.TopicName);
                else
                    return _lazyNamespaceManager.Value.DeleteSubscriptionAsync(name.TopicName,
                        name.SubscriptionName);
            }
        }

        public async Task<bool> QueueExistsAsync(QueueName name)
        {
            if (name.IsSimpleQueue)
                return await _lazyNamespaceManager.Value.QueueExistsAsync(name.TopicName);
            else
            {
                if (name.IsTopic)
                    return await _lazyNamespaceManager.Value.TopicExistsAsync(name.TopicName);
                else
                    return
                        await _lazyNamespaceManager.Value.TopicExistsAsync(name.TopicName) &&
                        await _lazyNamespaceManager.Value.SubscriptionExistsAsync(name.TopicName,
                        name.SubscriptionName);
            }
        }

        public Task KeepExtendingLeaseAsync(Event message, TimeSpan howLong, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs args)
        {
            TheTrace.TraceError("Boy this should have never happened since we have exception handling in Factory Actor: {}", args.Exception);
            return Task.FromResult(false);
        }

        private async Task ProcessMessage(Message m, CancellationToken cancellationToken,
            Func<Event, IEventQueueOperator, Task> handler, ActorDescriptor descriptor)
        {
            var name = new QueueName(descriptor.SourceQueueName);
            await handler(m.ToEvent(name), this);
        }

        public void RegisterHandler(Func<Event, IEventQueueOperator, Task> handler, ActorDescriptor descriptor)
        {
            var name = new QueueName(descriptor.SourceQueueName);
            Func<Message, CancellationToken, Task> h = (Message m, CancellationToken token) => ProcessMessage(m, token, handler, descriptor);
            var options = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = descriptor.DegreeOfParallelism,
                MaxAutoRenewDuration = descriptor.MaximumLease.HasValue ? descriptor.MaximumLease.Value : _maxAutoRenew,
                AutoComplete = true
            };

            if (name.IsSimpleQueue)
            {
                var client = _clientProvider.GetQueueClient(name);
                client.RegisterMessageHandler(h, options);
            }
            else
            {
                var client = _clientProvider.GetSubscriptionClient(name);
                client.RegisterMessageHandler(h, options);
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
                   new SubscriptionClient(_connectionString,
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
                    new TopicClient(_connectionString,
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
                    new QueueClient(_connectionString,
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
