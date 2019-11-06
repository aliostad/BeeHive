using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BeeHive.Azure.Tests.Integration
{
    public class AzureServiceBusTests
    {
        string ConnectionString = null;

        public AzureServiceBusTests()
        {
            ConnectionString = Environment.GetEnvironmentVariable(EnvVars.ConnectionStrings.ServiceBus);
        }

        [EnvVarIgnoreFact(EnvVars.ConnectionStrings.ServiceBus)]
        public async Task TopicCreateAndExists()
        {
            var topicName = Guid.NewGuid().ToString("N");
            var queueName = QueueName.FromTopicName(topicName);
            var serviceBusOperator = new ServiceBusOperator(ConnectionString);
            Assert.False(await serviceBusOperator.QueueExistsAsync(queueName));

            await serviceBusOperator.CreateQueueAsync(queueName);
            Assert.True(await serviceBusOperator.QueueExistsAsync(queueName));

            await serviceBusOperator.DeleteQueueAsync(queueName);
            Assert.False(await serviceBusOperator.QueueExistsAsync(queueName));

        }

        [EnvVarIgnoreFact(EnvVars.ConnectionStrings.ServiceBus)]
        public async Task TopicAndSubscriptionCreateAndExists()
        {
            var topicName = Guid.NewGuid().ToString("N");
            var subName = Guid.NewGuid().ToString("N");
            var topicQueueName = QueueName.FromTopicName(topicName);
            var queueName = QueueName.FromTopicAndSubscriptionName(topicName, subName);
            var serviceBusOperator = new ServiceBusOperator(ConnectionString);


            Assert.False(await serviceBusOperator.QueueExistsAsync(queueName));

            await serviceBusOperator.CreateQueueAsync(topicQueueName);
            await serviceBusOperator.CreateQueueAsync(queueName);
            Assert.True(await serviceBusOperator.QueueExistsAsync(queueName));

            await serviceBusOperator.DeleteQueueAsync(queueName);
            await serviceBusOperator.DeleteQueueAsync(topicQueueName);
            Assert.False(await serviceBusOperator.QueueExistsAsync(queueName));


        }

        [EnvVarIgnoreFact(EnvVars.ConnectionStrings.ServiceBus)]
        public async Task TopicAndSubscriptionCreateAndSent()
        {
            var topicName = Guid.NewGuid().ToString("N");
            var subName = Guid.NewGuid().ToString("N");
            var topicQueueName = QueueName.FromTopicName(topicName);
            var queueName = QueueName.FromTopicAndSubscriptionName(topicName, subName);
            var serviceBusOperator = new ServiceBusOperator(ConnectionString);


            await serviceBusOperator.CreateQueueAsync(topicQueueName);
            await serviceBusOperator.CreateQueueAsync(queueName);

            await serviceBusOperator.PushAsync(new Event("chashm")
            {
               QueueName = topicQueueName.ToString()
            });

            await serviceBusOperator.DeleteQueueAsync(queueName);
            await serviceBusOperator.DeleteQueueAsync(topicQueueName);


        }

        [EnvVarIgnoreFact(EnvVars.ConnectionStrings.ServiceBus)]
        public async Task TopicAndSubscriptionCreateAndSentBatch()
        {
            var topicName = Guid.NewGuid().ToString("N");
            var subName = Guid.NewGuid().ToString("N");
            var topicQueueName = QueueName.FromTopicName(topicName);
            var queueName = QueueName.FromTopicAndSubscriptionName(topicName, subName);
            var serviceBusOperator = new ServiceBusOperator(ConnectionString);


            await serviceBusOperator.CreateQueueAsync(topicQueueName);
            await serviceBusOperator.CreateQueueAsync(queueName);

            await serviceBusOperator.PushBatchAsync(new[]{ new Event("chashm")
            {
                QueueName = topicQueueName.ToString()
            },
            new Event("chashm")
            {
                QueueName = topicQueueName.ToString()
            }
            });

            await serviceBusOperator.DeleteQueueAsync(queueName);
            await serviceBusOperator.DeleteQueueAsync(topicQueueName);

        }



        [EnvVarIgnoreFact(EnvVars.ConnectionStrings.ServiceBus)]
        public void NeverEverCreatesAMessageBiggerThan256KB()
        {
            const int BufferSize = 10*1024;
            const int NumberOfMessages = 1000;
            var random = new Random();
            var list = new List<Event>();
            for (int i = 0; i < NumberOfMessages; i++)
            {
                var fatMessage = new FatMessage() {ALotOfBytes = new byte[BufferSize + random.Next(10000)]};
                random.NextBytes(fatMessage.ALotOfBytes);
                list.Add(new Event(fatMessage));
            }

            var batchUp = ServiceBusOperator.BatchUp(list);
            foreach (var batch in batchUp)
            {
                var sum = batch.Sum(x => x.Size);
                Assert.True(sum < 256 * 1024, "Size BIg!" );
                Console.WriteLine("Size => {0}", sum);
            }

            Assert.Equal(NumberOfMessages, batchUp.Sum(x => x.Count));
        }



        class FatMessage
        {
            public byte[] ALotOfBytes { get; set; }
        }
    }
}
