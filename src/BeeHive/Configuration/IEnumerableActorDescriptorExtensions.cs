using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Configuration
{
    public static class IEnumerableActorDescriptorExtensions
    {
        private class NoNonsenseConfiguration : IActorConfiguration
        {
            private ActorDescriptor[] _descriptors;

            public NoNonsenseConfiguration(IEnumerable<ActorDescriptor> descriptors)
            {
                _descriptors = descriptors.ToArray();
            }

            public IEnumerable<ActorDescriptor> GetDescriptors()
            {
                return _descriptors;
            }
        }
        public static IActorConfiguration ToConfiguration(this IEnumerable<ActorDescriptor> descriptors)
        {
            return new NoNonsenseConfiguration(descriptors);
        }
    }
}
