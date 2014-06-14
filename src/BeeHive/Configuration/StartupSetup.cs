using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Configuration
{
    public static class StartupSetup
    {
        public static IEnumerable<IStartup> InAssembly<T>()
        {
            return InAssembly(Assembly.GetAssembly( typeof (T)));
        }

        public static IEnumerable<IStartup> InAssembly(Assembly assembly)
        {
            return assembly.GetExportedTypes()
                .Where(t => typeof (IStartup).IsAssignableFrom(t))
                .Select(x => (IStartup) Activator.CreateInstance(x));
        }

        public static void Start(this IEnumerable<IStartup> startups,
            IServiceLocator serviceLocator)
        {
            foreach (var startup in startups)
            {
                startup.Start(serviceLocator);
            }
        }
    }
}
