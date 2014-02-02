using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public Task PushAsync(Event message)
        {

            var q = _queues.GetOrAdd(message.EventType,
                new InMemoryQueue<Event>(message.EventType));
            

            return q.PushAsync(message);
        }

        public Task<PollerResult<Event>> NextAsync(string queueName)
        {
            var name = new QueueName(queueName);
            var q = GetQueue(name);

            return q.Subscriptions[name.SubscriptionName].NextAsync(name.SubscriptionName)
                .ContinueWith((task) =>
                {
                    if (!task.IsFaulted && task.Result.IsSuccessful)
                        task.Result.PollingResult.QueueName = queueName;

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

        public Task Abandon(Event message)
        {
            var name = new QueueName(message.QueueName);
            var q = GetQueue(name);
            return q.Subscriptions[name.SubscriptionName].Abandon(message);
        }

        public Task Commit(Event message)
        {
            var name = new QueueName(message.QueueName);
            var q = GetQueue(name);
            return q.Subscriptions[name.SubscriptionName].Commit(message);            
        }

        public Task CreateQueue(string topicName, params string[] subscriptions)
        {
            return Task.Run(() =>
            {
                if (_queues.ContainsKey(topicName))
                    throw new InvalidOperationException("Queue already exists");

                var queue = _queues.GetOrAdd(topicName, new InMemoryQueue<Event>(topicName));
                
                // simple queue
                if (subscriptions.Length == 0)
                {
                    queue.Subscriptions.GetOrAdd(topicName, new InMemorySubscription<Event>(topicName));
                }

                foreach (var subscription in subscriptions)
                {
                    queue.Subscriptions.GetOrAdd(topicName, new InMemorySubscription<Event>(subscription));
                }

            });
        }

        public Task DeleteQueue(string topicName)
        {
            InMemoryQueue<Event> deleted;
            return Task.Run(() => _queues.TryRemove(topicName, out deleted));
        }

        public Task AddSubscription(string topicName, string subscriptionName)
        {
            return Task.Run(() =>
            {
                var q = _queues.GetOrAdd(topicName, new InMemoryQueue<Event>(topicName));
                q.TryAddSubscription(topicName);
            });
        }

        public Task RemoveSubscription(string topicName, string subscriptionName)
        {
            return Task.Run(() =>
            {
                var q = _queues.GetOrAdd(topicName, new InMemoryQueue<Event>(topicName));
                InMemorySubscription<Event> removed;

                q.Subscriptions.TryRemove(subscriptionName, out removed);
            });
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

        internal void AcceptMessage(T message)
        {
            _messages.Enqueue(message);
            if(_cancellationTokenSource!=null)
                _cancellationTokenSource.Cancel();
        }

        public Task<PollerResult<T>> NextAsync(string queueName)
        {
            return Task.Factory.StartNew(() =>
            {
                _cancellationTokenSource = new CancellationTokenSource();
                T message;
                bool success = _messages.TryDequeue(out message);
                if (!success)
                {
                    Task.Delay(TimeSpan.FromSeconds(30), _cancellationTokenSource.Token).Wait();
                }
                _cancellationTokenSource = null;
                return new PollerResult<T>(success, message);
            });
        }

        public Task Abandon(T message)
        {
            return Task.Run(() =>
            {
                _messages.Enqueue(message);
                _leases.Remove(message);
            });
        }

        public Task Commit(T message)
        {
            return Task.Run(() =>
            {
                if (_leases.ContainsKey(message))
                    _leases.Remove(message);
            });
        }
    
    }


}
