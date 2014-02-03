using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive
{
    public class AppDomainExplorerActorConfiguration : IActorConfiguration
    {

        public IEnumerable<ActorDescriptor> GetDescriptors()
        {
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().
                SelectMany(x => x.ExportedTypes))
            {
                if (typeof (IProcessorActor).IsAssignableFrom(type))
                {
                    var attribute = type.GetCustomAttribute<ActorDescriptionAttribute>(true);
                    if (attribute != null)
                    {
                        yield return new ActorDescriptor()
                        {
                            ActorType = type,
                            DegreeOfParallelism = attribute.DegreeOfParallelism,
                            Interval = new RegularlyIncreasingInterval(TimeSpan.Zero, 
                                TimeSpan.FromSeconds(attribute.MaxIntervalSeconds), 10),
                                SourceQueueName = attribute.Name                             
                        };
                    }
                }
            }
        }
    }
}
