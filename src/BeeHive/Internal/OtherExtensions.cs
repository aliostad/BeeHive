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

            return types.Where(type => typeof (IProcessorActor).IsAssignableFrom(type))
                .SelectMany(type => type.GetCustomAttributes<ActorDescriptionAttribute>(true)
                    .Select(attribute => attribute.ToDescriptor(type)));

        }
    }
}
