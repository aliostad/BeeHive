using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BeeHive.Scheduling;

namespace BeeHive.ConsoleDemo
{
    internal class InMemoryServiceBus : IEventQueueOperator
    {
        private ConcurrentDictionary<string, InMemoryQueue<Event>> 
            _queues = new ConcurrentDictionary<string, InMemoryQueue<Event>>();

        public bool IsEventDriven => false;

        public Task PushAsync(Event message)
        {

            var q = _queues.GetOrAdd(message.EventType,
                new InMemoryQueue<Event>(message.EventType));
            

            return q.PushAsync(message);
        }

        public async Task PushBatchAsync(IEnumerable<Event> messages)
        {
            foreach (var message in messages)
            {
                await PushAsync(message);
            }
        }

        public Task<PollerResult<Event>> NextAsync(QueueName name)
        {
            var q = GetQueue(name);

            return q.Subscriptions[name.SubscriptionName].NextAsync(name)
                .ContinueWith((task) =>
                {
                    if (!task.IsFaulted && task.Result.IsSuccessful)
                        task.Result.PollingResult.QueueName = name.ToString();
                    else
                    {
                        
                    }
                    

                    return task.Result;
                });
        }

        private InMemoryQueue<Event> GetQueue(QueueName name)
        {
            var q = _queues.GetOrAdd(name.TopicName,
                (s) => new InMemoryQueue<Event>(s));
            q.TryAddSubscription(name.SubscriptionName);
            return q;
        }

        public Task AbandonAsync(Event message)
        {
            var name = new QueueName(message.QueueName);
            var q = GetQueue(name);
            return q.Subscriptions[name.SubscriptionName].AbandonAsync(message);
        }

        public Task CommitAsync(Event message)
        {
            var name = new QueueName(message.QueueName);
            var q = GetQueue(name);
            return q.Subscriptions[name.SubscriptionName].CommitAsync(message);            
        }

        public Task CreateQueueAsync(string topicName, params string[] subscriptions)
        {
            return Task.Run(() =>
            {

                var queue = _queues.GetOrAdd(topicName, new InMemoryQueue<Event>(topicName));
                
                // simple queue
                if (subscriptions.Length == 0)
                {
                    queue.Subscriptions.GetOrAdd(topicName, new InMemorySubscription<Event>(topicName));
                }

                foreach (var subscription in subscriptions)
                {
                    queue.Subscriptions.GetOrAdd(subscription, new InMemorySubscription<Event>(subscription));
                }

            });
        }

        public Task DeleteQueueAsync(string topicName)
        {
            InMemoryQueue<Event> deleted;
            return Task.Run(() => _queues.TryRemove(topicName, out deleted));
        }

        public Task AddSubscriptionAsync(string topicName, string subscriptionName)
        {
            return Task.Run(() =>
            {
                var q = _queues.GetOrAdd(topicName, new InMemoryQueue<Event>(subscriptionName));
                q.TryAddSubscription(topicName);
            });
        }

        public Task RemoveSubscriptionAsync(string topicName, string subscriptionName)
        {
            return Task.Run(() =>
            {
                var q = _queues.GetOrAdd(topicName, new InMemoryQueue<Event>(topicName));
                InMemorySubscription<Event> removed;

                q.Subscriptions.TryRemove(subscriptionName, out removed);
            });
        }

        public Task SetupQueueAsync(QueueName name)
        {
            return Task.Run(() =>
            {
                if (name.IsSimpleQueue)
                    CreateQueueAsync(name.TopicName);
                else
                    CreateQueueAsync(name.TopicName, name.SubscriptionName);
            });
           
        }

        public Task CreateQueueAsync(QueueName name)
        {
            if (name.IsSimpleQueue)
            {
                return CreateQueueAsync(name.TopicName);
            }
            else
            {
                return CreateQueueAsync(name.TopicName, name.SubscriptionName);
            }
        }

        public Task DeleteQueueAsync(QueueName name)
        {
            throw new NotImplementedException();
        }

        public Task<bool> QueueExistsAsync(QueueName name)
        {
            return Task.FromResult(_queues.ContainsKey(name.TopicName));
        }

        public Task DeferAsync(Event message, TimeSpan howLong)
        {
            throw new NotImplementedException();
        }

        public async Task KeepExtendingLeaseAsync(Event message, TimeSpan howLong, CancellationToken cancellationToken)
        {
            // OK
        }

        public void RegisterHandler(Func<Event, IEventQueueOperator, Task> handler, ActorDescriptor descriptor)
        {
            throw new NotSupportedException();
        }
    }

    internal class InMemoryQueue<T> : ITopicOperator<T>
        where T : ICloneable
    {

        private readonly ConcurrentDictionary<string, InMemorySubscription<T>> _subscriptions = new ConcurrentDictionary<string, InMemorySubscription<T>>();
        private readonly string _name;

        
        public InMemoryQueue(string name)
        {
            _name = name;    
        }

        public string Name
        {
            get { return _name; }
        }

        public ConcurrentDictionary<string, InMemorySubscription<T>> Subscriptions
        {
            get { return _subscriptions; }
        }

        public bool IsSimpleQueue()
        {
            return _subscriptions.Count == 1 &&
                   _subscriptions.Values.First().Name == Name;
        }

        public void TryAddSubscription(string name)
        {

            if (IsSimpleQueue())
                return;

            Subscriptions.GetOrAdd(name, new InMemorySubscription<T>(name));

        }
      

        public Task PushAsync(T message)
        {
            return Task.Run(() =>
            {
                foreach (var subscription in Subscriptions.Values)
                {
                    subscription.AcceptMessage((T) message.Clone());
                }
            });

        }

        public async Task PushBatchAsync(IEnumerable<T> messages)
        {
            foreach (var message in messages)
            {
                await PushAsync(message);
            }
        }
    }

    internal class InMemorySubscription<T> : ISubscriptionOperator<T>
    {
        private string _name;
        private ConcurrentQueue<T> _messages = new ConcurrentQueue<T>(); 
        private Dictionary<T,T> _leases = new Dictionary<T, T>();
        private CancellationTokenSource _cancellationTokenSource;

        public InMemorySubscription(string name)
        {
            Name = name;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public bool IsEventDriven => false;

        internal void AcceptMessage(T message)
        {
            _messages.Enqueue(message);
            if(_cancellationTokenSource!=null)
                _cancellationTokenSource.Cancel();
        }

        public async Task<PollerResult<T>> NextAsync(QueueName name)
        {
            T message;
            bool success = _messages.TryDequeue(out message);
            return new PollerResult<T>(success, message);
        }

        public Task AbandonAsync(T message)
        {
            return Task.Run(() =>
            {
                _messages.Enqueue(message);
                _leases.Remove(message);
            });
        }

        public Task CommitAsync(T message)
        {
            return Task.Run(() =>
            {
                if (_leases.ContainsKey(message))
                    _leases.Remove(message);
            });
        }

        public Task DeferAsync(T message, TimeSpan howLong)
        {
            throw new NotSupportedException();
        }

        public void RegisterHandler(Func<Event, Task<IEnumerable<Event>>> handler, ActorDescriptor descriptor)
        {
            throw new NotSupportedException();
        }

        public Task KeepExtendingLeaseAsync(T message, TimeSpan howLong, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public void RegisterHandler(Func<Event, IEventQueueOperator, Task> handler, ActorDescriptor descriptor)
        {
            throw new NotSupportedException();
        }
    }


}
