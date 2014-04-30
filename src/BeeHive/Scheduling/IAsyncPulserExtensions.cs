using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeeHive.Scheduling
{
    public static class IAsyncPulserExtensions
    {
        public static IPulser ToPulser(this IAsyncPulser asyncPulser)
        {

            if(asyncPulser==null)
                throw new ArgumentNullException("asyncPulser");

            var attribute = asyncPulser.GetType().GetCustomAttributes<PulserIntervalAttribute>().FirstOrDefault();
            if(attribute == null)
                throw new InvalidOperationException("This class is not decorated with PulserIntervalAttribute: " + asyncPulser.GetType());

            return new AsyncPoller(new FixedInterval(attribute.Interval),
                 (Func<CancellationToken, Task<IEnumerable<Event>>>)asyncPulser.Pulse);
        }
    }
}
