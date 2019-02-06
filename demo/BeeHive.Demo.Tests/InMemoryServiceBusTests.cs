using BeeHive.ConsoleDemo;
using Xunit;

namespace BeeHive.Tests.Demo
{
    public class InMemoryServiceBusTests
    {
        [Fact]
        public void TestSimpleQueue()
        {
            const string TheQueueName = "Hello";

            var inMemoryServiceBus = new InMemoryServiceBus();
            inMemoryServiceBus.CreateQueueAsync(TheQueueName);
            inMemoryServiceBus.PushAsync(new Event("Haya")
            {
                EventType = TheQueueName
            }).Wait();

            var pollerResult = inMemoryServiceBus.NextAsync(QueueName.FromSimpleQueueName(TheQueueName)).Result;
            Assert.True(pollerResult.IsSuccessful);
            Assert.Equal(TheQueueName, pollerResult.PollingResult.QueueName);
            Assert.Equal(TheQueueName, pollerResult.PollingResult.EventType);
            Assert.Equal("Haya", pollerResult.PollingResult.GetBody<string>());
        }
    }
}
