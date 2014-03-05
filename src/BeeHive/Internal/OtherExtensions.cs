using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Actors;

namespace BeeHive.Internal
{
    internal static class OtherExtensions
    {
        public static IEnumerable<ActorDescriptor> GetActorDescriptors(this IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                if (typeof(IProcessorActor).IsAssignableFrom(type))
                {
                    var attribute = type.GetCustomAttribute<ActorDescriptionAttribute>(true);
                    if (attribute != null)
                    {
                        yield return attribute.ToDescriptor(type);
                    }
                }
            }
            
        }
    }
}
