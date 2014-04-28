using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Scheduling;

namespace BeeHive.Actors
{
    public static class ActorDescriptionAttributeExtensions
    {
        public static ActorDescriptor ToDescriptor(this ActorDescriptionAttribute attribute, Type actorType)
        {
            return new ActorDescriptor()
            {
                ActorType = actorType,
                DegreeOfParallelism = attribute.DegreeOfParallelism,
                Interval = new RegularlyIncreasingInterval(TimeSpan.Zero,
                    TimeSpan.FromSeconds(attribute.MaxIntervalSeconds), 10),
                SourceQueueName = attribute.Name
            };
        }
    }
}
