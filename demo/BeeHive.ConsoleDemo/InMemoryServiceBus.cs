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

            var q = _queues.GetOrAdd(message.EventType, (name) =>
            {
                return new InMemoryQueue<Event>(name);
            });

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
    }

    internal class InMemoryQueue<T> : ITopicOperator<T>
        where T : ICloneable
    {

        private readonly ConcurrentDictionary<string, Subscription<T>> _subscriptions = new ConcurrentDictionary<string, Subscription<T>>();
        private readonly string _name;

        
        public InMemoryQueue(string name)
        {
            _name = name;    
        }

        public string Name
        {
            get { return _name; }
        }

        public ConcurrentDictionary<string, Subscription<T>> Subscriptions
        {
            get { return _subscriptions; }
        }


        public void TryAddSubscription(string name)
        {
            Subscriptions.GetOrAdd(name, new Subscription<T>(name));
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

    internal class Subscription<T> : ISubscriptionOperator<T>
    {
        private string _name;
        private ConcurrentQueue<T> _messages = new ConcurrentQueue<T>(); 
        private Dictionary<T,T> _leases = new Dictionary<T, T>();
        private CancellationTokenSource _cancellationTokenSource;

        public Subscription(string name)
        {
            _name = name;
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
