using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeeHive.Scheduling
{
    /// <summary>
    /// Implement your custom pulser by impl this interface and decorate with PulserDescriptionAttribute
    /// </summary>
    public interface IAsyncPulser
    {
        Task<IEnumerable<Event>> Pulse(CancellationToken token);
    }
}
