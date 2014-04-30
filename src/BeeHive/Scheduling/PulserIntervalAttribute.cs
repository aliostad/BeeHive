using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Scheduling
{

    /// <summary>
    /// Decorate an instance of IAsyncPulser with this
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PulserIntervalAttribute : Attribute
    {
        public PulserIntervalAttribute(int intervalInSeconds)
        {
            Interval = TimeSpan.FromSeconds(intervalInSeconds);
        }

        public TimeSpan Interval { get; private set; }
    }
}
