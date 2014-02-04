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
        private string _assemblyPrefix;

        public AppDomainExplorerActorConfiguration(string assemblyPrefix = "")
        {
            _assemblyPrefix = assemblyPrefix;
        }

        public IEnumerable<ActorDescriptor> GetDescriptors()
        {
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().
                Where(x => x.FullName.StartsWith(_assemblyPrefix)).
                SelectMany(y => y.ExportedTypes))
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
