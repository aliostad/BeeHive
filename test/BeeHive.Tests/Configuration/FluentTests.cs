using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Configuration;
using BeeHive.Tests.Data;
using Xunit;

namespace BeeHive.Tests.Configuration
{

    public class FluentTests
    {

        public class NoAttribActor : IProcessorActor
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<Event>> ProcessAsync(Event evnt)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void FromThisAssembly_Does_NOT_Return_DummyActor()
        {
            var descriptors = ActorDescriptors.FromThisAssembly();
            Assert.True(descriptors.All(x => x.ActorType != typeof(NoAttribActor)));
        }

        [Fact]
        public void FromThisAssembly_Returns_DummyActor()
        {
            var descriptors = ActorDescriptors.FromThisAssembly();
            Assert.True(descriptors.Any(x => x.ActorType == typeof(DummyActor) ));
        }

        [Fact]
        public void FromAssemblyContaing_Returns_DummyActor()
        {
            var type = typeof(BeeHive.ConsoleDemo.DummyActor);
            var descriptors = ActorDescriptors.FromAssemblyContaining(type);
            Assert.True(descriptors.Any(x => x.ActorType == type));
        }

    }
}
