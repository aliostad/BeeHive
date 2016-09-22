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

        /// <summary>
        /// Updates the DegreeOfParallelism with entry from configuration in the form of Beehive.ActorParallelism.{ActorName}
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="configurationValueProvider"></param>
        /// <returns>an updated configuration</returns>
        public static IActorConfiguration UpdateParallelism(this IActorConfiguration configuration, IConfigurationValueProvider configurationValueProvider)
        {
            var descriptors = configuration.GetDescriptors()
                .Select(descriptor =>
                {
                    string value =
                        configurationValueProvider.GetValue("Beehive.ActorParallelism." + descriptor.ActorType.Name);
                    int parallelism = descriptor.DegreeOfParallelism;

                    if (int.TryParse(value, out parallelism))
                        descriptor.DegreeOfParallelism = parallelism;

                    return descriptor;
                });

            return new NoNonsenseConfiguration(descriptors);
        }
    }
}
