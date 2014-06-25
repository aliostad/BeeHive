using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BeeHive.Azure.Tests
{
    public class AzureServiceBusTests
    {

        private const string ConnectionString =
            "Endpoint=sb://beehivetest.servicebus.windows.net/;SharedSecretIssuer=owner;SharedSecretValue=FIFK7e6S7TgDX2q2zz+QYpNybFYaaCdZpKfKJLKb42c=";

        [Fact]
        public void TopicCreateAndExists()
        {
            var topicName = Guid.NewGuid().ToString("N");
            var queueName = QueueName.FromTopicName(topicName);
            var serviceBusOperator = new ServiceBusOperator(ConnectionString);
            Assert.False(serviceBusOperator.QueueExistsAsync(queueName).Result);

            serviceBusOperator.CreateQueueAsync(queueName).Wait();
            Assert.True(serviceBusOperator.QueueExistsAsync(queueName).Result);

            serviceBusOperator.DeleteQueueAsync(queueName).Wait();
            Assert.False(serviceBusOperator.QueueExistsAsync(queueName).Result);

        }

        [Fact]
        public void TopicAndSubscriptionCreateAndExists()
        {
            var topicName = Guid.NewGuid().ToString("N");
            var subName = Guid.NewGuid().ToString("N");
            var topicQueueName = QueueName.FromTopicName(topicName);
            var queueName = QueueName.FromTopicAndSubscriptionName(topicName, subName);
            var serviceBusOperator = new ServiceBusOperator(ConnectionString);


            Assert.False(serviceBusOperator.QueueExistsAsync(queueName).Result);

            serviceBusOperator.CreateQueueAsync(topicQueueName).Wait();
            serviceBusOperator.CreateQueueAsync(queueName).Wait();
            Assert.True(serviceBusOperator.QueueExistsAsync(queueName).Result);

            serviceBusOperator.DeleteQueueAsync(queueName).Wait();
            serviceBusOperator.DeleteQueueAsync(topicQueueName).Wait();
            Assert.False(serviceBusOperator.QueueExistsAsync(queueName).Result);


        }

        [Fact]
        public void TopicAndSubscriptionCreateAndSent()
        {
            var topicName = Guid.NewGuid().ToString("N");
            var subName = Guid.NewGuid().ToString("N");
            var topicQueueName = QueueName.FromTopicName(topicName);
            var queueName = QueueName.FromTopicAndSubscriptionName(topicName, subName);
            var serviceBusOperator = new ServiceBusOperator(ConnectionString);


            serviceBusOperator.CreateQueueAsync(topicQueueName).Wait();
            serviceBusOperator.CreateQueueAsync(queueName).Wait();

            serviceBusOperator.PushAsync(new Event("chashm")
            {
               QueueName = topicQueueName.ToString()
            }).Wait();

            serviceBusOperator.DeleteQueueAsync(queueName).Wait();
            serviceBusOperator.DeleteQueueAsync(topicQueueName).Wait();


        }

        [Fact]
        public void TopicAndSubscriptionCreateAndSentBatch()
        {
            var topicName = Guid.NewGuid().ToString("N");
            var subName = Guid.NewGuid().ToString("N");
            var topicQueueName = QueueName.FromTopicName(topicName);
            var queueName = QueueName.FromTopicAndSubscriptionName(topicName, subName);
            var serviceBusOperator = new ServiceBusOperator(ConnectionString);


            serviceBusOperator.CreateQueueAsync(topicQueueName).Wait();
            serviceBusOperator.CreateQueueAsync(queueName).Wait();

            serviceBusOperator.PushBatchAsync(new[]{ new Event("chashm")
            {
                QueueName = topicQueueName.ToString()
            },
            new Event("chashm")
            {
                QueueName = topicQueueName.ToString()
            }
            }).Wait();

            serviceBusOperator.DeleteQueueAsync(queueName).Wait();
            serviceBusOperator.DeleteQueueAsync(topicQueueName).Wait();


        }

    }
}
