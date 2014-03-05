using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Actors;
using BeeHive.Internal;

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
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.FullName.StartsWith(_assemblyPrefix))
                .SelectMany(y => y.ExportedTypes)
                    .GetActorDescriptors();

        }
        
    }
}
