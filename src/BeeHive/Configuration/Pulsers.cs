using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Scheduling;

namespace BeeHive.Configuration
{
    public static class Pulsers
    {
        public static IEnumerable<IPulser> GetSimplePulers(
            this Assembly assembly)
        {
            return assembly.GetCustomAttributes(typeof (SimpleAutoPulserDescriptionAttribute))
                .Cast<SimpleAutoPulserDescriptionAttribute>()
                .Select(x => new SimpleAutoPulser(TimeSpan.FromSeconds(x.IntervalSeconds), x.EventType));
        }



    }
}
