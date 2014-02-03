using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive
{
    public static class IServiceLocatorExtensions
    {
        public static T GetService<T>(this IServiceLocator serviceLocator)
        {
            return (T)serviceLocator.GetService(typeof(T));
        }
        public static IEnumerable<T> GetServices<T>(this IServiceLocator serviceLocator)
        {
            return serviceLocator.GetServices(typeof(T))
                .Cast<T>();
        }
         
    }
}
