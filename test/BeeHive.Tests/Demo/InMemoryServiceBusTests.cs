using BeeHive.ConsoleDemo;
using Xunit;

namespace BeeHive.Tests.Demo
{
    public class InMemoryServiceBusTests
    {
        [Fact]
        public void TestSimpleQueue()
        {
            const string QueueName = "Hello";

            var inMemoryServiceBus = new InMemoryServiceBus();
            inMemoryServiceBus.CreateQueueAsync(QueueName);
            inMemoryServiceBus.PushAsync(new Event("Haya")
            {
                EventType = QueueName
            }).Wait();

            var pollerResult = inMemoryServiceBus.NextAsync(QueueName).Result;
            Assert.True(pollerResult.IsSuccessful);
            Assert.Equal(QueueName, pollerResult.PollingResult.QueueName);
            Assert.Equal(QueueName, pollerResult.PollingResult.EventType);
            Assert.Equal("Haya", pollerResult.PollingResult.GetBody<string>());
        }
    }
}
