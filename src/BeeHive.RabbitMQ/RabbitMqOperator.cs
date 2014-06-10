using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Scheduling;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BeeHive.RabbitMQ
{
    public class RabbitMqOperator : IEventQueueOperator
    {
        private IConnectionProvider _connectionProvider;
        private TimeSpan _longPollingTimeout;

        private const string FanOut = "fanout";

        public RabbitMqOperator(IConnectionProvider connectionProvider):
            this(connectionProvider, TimeSpan.FromSeconds(30))
        {
        }

        public RabbitMqOperator(IConnectionProvider connectionProvider,
            TimeSpan longPollingTimeout)
        {
            _longPollingTimeout = longPollingTimeout;
            _connectionProvider = connectionProvider;
        }

        public Task PushAsync(Event message)
        {
            var connection = _connectionProvider.GetConnection();
            using (var channel = connection.CreateModel())
            {
            
                channel.BasicPublish(string.Empty,
                    message.QueueName,
                    null,
                    Encoding.UTF8.GetBytes(message.Body)); 
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Does not support batch and only loops through
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public Task PushBatchAsync(IEnumerable<Event> messages)
        {
            var connection = _connectionProvider.GetConnection();
            using (var channel = connection.CreateModel())
            {
                var consumer = new QueueingBasicConsumer(channel);
                foreach (var message in messages)
                {
                    channel.BasicPublish(message.QueueName,
                        string.Empty,
                        null,
                        Encoding.UTF8.GetBytes(message.Body));    
                }
            }

            return Task.FromResult(0);
        }

        public Task<PollerResult<Event>> NextAsync(QueueName name)
        {
            var connection = _connectionProvider.GetConnection();
            var result = new PollerResult<Event>(false , Event.Empty);
            using (var channel = connection.CreateModel())
            {
                var consumer = new QueueingBasicConsumer(channel);
                channel.BasicConsume(name.SubscriptionName, true, consumer);
                BasicDeliverEventArgs eventArgs = null;
                if (consumer.Queue.Dequeue((int)_longPollingTimeout.TotalMilliseconds, 
                    out eventArgs))
                {
                    var @event = new Event()
                    {
                        Body = Encoding.UTF8.GetString(eventArgs.Body),
                        QueueName = name.SubscriptionName,
                        UnderlyingMessage = eventArgs,
                        ContentType = "text/plain", // should it be JSON?
                        EventType = name.SubscriptionName
                    };

                    result = new PollerResult<Event>(true, @event);
                }
                return Task.FromResult(result);
            }

        }

        public Task AbandonAsync(Event message)
        {
            var underlyingMessage = (BasicDeliverEventArgs) message.UnderlyingMessage;
            var connection = _connectionProvider.GetConnection();

            using (var channel = connection.CreateModel())
            {
                channel.BasicNack(underlyingMessage.DeliveryTag, false, true); // TODO: implement max error count
            }

            return Task.FromResult(0);
        }

        public Task CommitAsync(Event message)
        {
            var underlyingMessage = (BasicDeliverEventArgs)message.UnderlyingMessage;
            var connection = _connectionProvider.GetConnection();

            using (var channel = connection.CreateModel())
            {
                channel.BasicAck(underlyingMessage.DeliveryTag, false); 
            }

            return Task.FromResult(0);
        }

        public Task DeferAsync(Event message, TimeSpan howLong)
        {
            throw new NotSupportedException(); // TODO: investigate
        }

        public Task CreateQueueAsync(QueueName name)
        {

            var connection = _connectionProvider.GetConnection();
            using (var channel = connection.CreateModel())
            {
                if (name.IsSimpleQueue)
                {
                    var queueDeclareOk = channel.QueueDeclare(name.TopicName,
                        true,
                        false,
                        false,
                        new Dictionary<string, object>());
                }
                else
                {
                    if (name.IsTopic)
                    {
                        channel.ExchangeDeclare(name.TopicName, FanOut, true);
                    }
                    else
                    {
                        channel.ExchangeDeclare(name.TopicName, FanOut, true);
                        var queueDeclareOk = channel.QueueDeclare(name.SubscriptionName,
                            true,
                            false,
                            true,
                            new Dictionary<string, object>());    
                        channel.QueueBind(name.SubscriptionName,
                            name.TopicName,
                            string.Empty);
                    }
                }
            }
            return Task.FromResult(0);
        }

        public Task DeleteQueueAsync(QueueName name)
        {

            var connection = _connectionProvider.GetConnection();
            using (var channel = connection.CreateModel())
            {
                if (name.IsTopic)
                {
                    channel.ExchangeDelete(name.TopicName);
                }
                else
                {
                    channel.QueueDelete(name.SubscriptionName);
                }
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// There is no way to check if a queue exists 
        /// so we return false so queue is created.
        /// Creation of queue is idempotent in RabbitMQ.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Task<bool> QueueExists(QueueName name)
        {
            return Task.FromResult(false);
        }
    }
}
