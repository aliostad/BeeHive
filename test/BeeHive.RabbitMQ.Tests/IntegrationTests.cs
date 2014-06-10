using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Xunit;

namespace BeeHive.RabbitMQ.Tests
{
    public class IntegrationTests
    {

        [Fact]
        public void CanCreateSendAndReadFromSimpleQueue()
        {

            string q = "shabash";
            string content = "shaki";
            var queueName = QueueName.FromSimpleQueueName(q);
            var operat = new RabbitMqOperator(
                new ConnectionProvider(new ConnectionFactory()));
            operat.DeleteQueueAsync(queueName).Wait();
            
            
            operat.CreateQueueAsync(queueName).Wait();
            operat.PushAsync(new Event(content)
                {
                    EventType = q,
                    QueueName = q
                }).Wait();

            var result = operat.NextAsync(queueName).Result;
            Assert.True(result.IsSuccessful);
            operat.CommitAsync(result.PollingResult).Wait();
            Assert.Equal(content, result.PollingResult.GetBody<string>());
        }


        [Fact]
        public void CanCreateSendAndReadFromPubSubQueue()
        {

            string q = "skorantes-volidang";
            string content = "shaki";
            var queueName = new QueueName(q);
            var operat = new RabbitMqOperator(
                new ConnectionProvider(new ConnectionFactory()));
            operat.DeleteQueueAsync(queueName).Wait();


            operat.CreateQueueAsync(queueName).Wait();
            operat.PushAsync(new Event(content)
            {
                EventType = queueName.SubscriptionName,
                QueueName = queueName.SubscriptionName
            }).Wait();

            var result = operat.NextAsync(queueName).Result;
            Assert.True(result.IsSuccessful);
            operat.CommitAsync(result.PollingResult).Wait();
            Assert.Equal(content, result.PollingResult.GetBody<string>());
        }
    }
}
