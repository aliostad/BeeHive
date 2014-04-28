using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeeHive.Scheduling
{
    public interface IAsyncPulser
    {
        Task<IEnumerable<Event>> Pulse(CancellationToken token);
    }
}
