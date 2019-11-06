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
        private const string EnvVar = "rabbitmq_service_running"; 

        [EnvVarIgnoreFact(EnvVar)]
        public async Task CanCreateSendAndReadFromSimpleQueue()
        {

            string q = "shabash";
            string content = "shaki";
            var queueName = QueueName.FromSimpleQueueName(q);
            var operat = new RabbitMqOperator(
                new ConnectionProvider(new ConnectionFactoryWrapper(
                    new ConnectionFactory())));
            await operat.DeleteQueueAsync(queueName);
            
            
            await operat.CreateQueueAsync(queueName);
            await operat.PushAsync(new Event(content)
                {
                    EventType = q,
                    QueueName = q
                });

            var result = await operat.NextAsync(queueName);
            Assert.True(result.IsSuccessful);
            await operat.CommitAsync(result.PollingResult);
            Assert.Equal(content, result.PollingResult.GetBody<string>());
        }


        [EnvVarIgnoreFact(EnvVar)]
        public async Task CanCreateSendAndReadFromPubSubQueue()
        {

            string q = "skorantes-volidang";
            string content = "shaki";
            var queueName = new QueueName(q);
            var operat = new RabbitMqOperator(
                new ConnectionProvider(new ConnectionFactoryWrapper(new ConnectionFactory())));
            await operat.DeleteQueueAsync(queueName);


            await operat.CreateQueueAsync(queueName);
            await operat.PushAsync(new Event(content)
            {
                EventType = queueName.SubscriptionName,
                QueueName = queueName.SubscriptionName
            });

            var result = await operat.NextAsync(queueName);
            Assert.True(result.IsSuccessful);
            await operat.CommitAsync(result.PollingResult);
            Assert.Equal(content, result.PollingResult.GetBody<string>());
        }
    }

    class EnvVarIgnoreFactAttribute : FactAttribute
    {
        public EnvVarIgnoreFactAttribute(string envVar)
        {
            var env = Environment.GetEnvironmentVariable(envVar);
            if (string.IsNullOrEmpty(env))
            {
                Skip = $"Please set {envVar} env var to run.";
            }
        }
    }
}
