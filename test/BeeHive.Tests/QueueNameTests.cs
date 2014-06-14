using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

namespace BeeHive.Tests
{
    public class QueueNameTests
    {
        [Theory]
        [InlineData("Simple Queue", "SimpleQueueName", "SimpleQueueName", "SimpleQueueName")]
        [InlineData("Topic Queue", "Topic-Subscription", "Topic", "Subscription")]
        [InlineData("Multiple hyphen", "Simple-Queue-Name", "", "")]
        [InlineData("underscore and number fine", "1122fwef_fwef-23huh_buhasi", "1122fwef_fwef", "23huh_buhasi")]
        public void TestCases(string caseName,
            string queueName, 
            string expectedTopicName, 
            string expectedSubscriptionName)
        {
            try
            {
                var name = new QueueName(queueName);
                Assert.Equal(expectedTopicName, name.TopicName);
                Assert.Equal(expectedSubscriptionName, name.SubscriptionName);
            }
            catch (ArgumentException exception)
            {
                if((expectedTopicName + expectedSubscriptionName).Length > 1)
                   Assert.True(false, caseName + 
                       " " + exception.ToString());
            }
        }

        [Fact]
        public void FromTopicName()
        {
            var name = QueueName.FromTopicName("Ali");
            Assert.True(name.IsTopic);
            Assert.False(name.IsSimpleQueue);
        }

        [Fact]
        public void FromSimpleQueueName()
        {
            var name = QueueName.FromSimpleQueueName("Ali");
            Assert.True(name.IsSimpleQueue);
            Assert.False(name.IsTopic);
        }
    }
}
