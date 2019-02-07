using BeeHive.Azure;
using BeeHive.Azure.Functions;
using BeeHive.DataStructures;
using BeeHive.Demo.PrismoEcommerce.Actors;
using BeeHive.Demo.PrismoEcommerce.Entities;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BeeHive.Demo.PrismoEcommerce.Functions
{
    /// <summary>
    /// Example 
    /// </summary>
    public static class Funcs
    {
        class ConfigKeys
        {
            public const string StorageConnectionString = "StorageConnectionString";
            public const string ServiceBusConnectionString = "ServiceBusConnectionString";
        }

        [FunctionName("ExamplePulser")]
        public async static Task ExamplePulser([TimerTrigger("0 1/10 * * * *")]TimerInfo info, ILogger log)
        {
            await GetOperator().PushAsync(new Event("does not matter")
            {
                QueueName = QueueName.FromTopicName("PulseTopic").ToString()
            });
        }

        static ICollectionStore<T> GetCollectionStore<T>()
            where T : IHaveIdentity
        {
            return new AzureCollectionStore<T>(Environment.GetEnvironmentVariable(ConfigKeys.StorageConnectionString));
        }

        static IEventQueueOperator GetOperator()
        {
            return new ServiceBusOperator(Environment.GetEnvironmentVariable(ConfigKeys.ServiceBusConnectionString));
        }


        [FunctionName("FraudCancelOrderActor")]
        public static Task FeedCaptureSignalled_FeedCapture(
            [ServiceBusTrigger("FrauCheckFailed", "CancelOrder", Connection = ConfigKeys.ServiceBusConnectionString)]Message msg,
            ILogger log)
        {
            var actor = new FraudCancelOrderActor(GetCollectionStore<Order>());
            return actor.Invoke(new QueueName("FrauCheckFailed-CancelOrder"), msg, GetOperator());
        }

    }
}
