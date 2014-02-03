using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Tests.Data;
using Xunit;

namespace BeeHive.Tests
{
    public class AppDomainExplorerActorConfigurationTests
    {
        [Fact]
        public void Test()
        {
            var configuration = new AppDomainExplorerActorConfiguration();
            var actorDescriptors = configuration.GetDescriptors();
            var descriptor = actorDescriptors.First();
            Assert.Equal(typeof(DummyActor), descriptor.ActorType);
            Assert.Equal(3, descriptor.DegreeOfParallelism);
            Assert.Equal("Dummy", descriptor.SourceQueueName);
        }
    }
}
