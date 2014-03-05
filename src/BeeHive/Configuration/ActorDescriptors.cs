using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BeeHive.Internal;

namespace BeeHive.Configuration
{
    public class ActorDescriptors
    {

        public static IEnumerable<ActorDescriptor> FromThisAssembly()
        {
            var stackTrace = new StackTrace();
            var method = stackTrace.GetFrames()[1].GetMethod();
            return FromAssemblyContaining(method.DeclaringType);
        }

        public static IEnumerable<ActorDescriptor> FromAssemblyContaining(Type type)
        {
            return Assembly.GetAssembly(type).ExportedTypes
                    .GetActorDescriptors();
        }

        public static IEnumerable<ActorDescriptor> FromAssemblyContaining<T>()
        {
            return FromAssemblyContaining(typeof (T));
        }

    }
}
