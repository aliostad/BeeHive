using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Configuration
{
    public class AggregateActorConfiguration : IActorConfiguration
    {
        private IEnumerable<IActorConfiguration> _configurations;

        public AggregateActorConfiguration(IEnumerable<IActorConfiguration> configurations)
        {
            _configurations = configurations;
        }

        public IEnumerable<ActorDescriptor> GetDescriptors()
        {
            return _configurations.SelectMany(c => c.GetDescriptors());
        }
    }
}
