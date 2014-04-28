using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Scheduling
{
    public interface IPulser : IService
    {
        event EventHandler<IEnumerable<Event>> PulseGenerated;       
    }
}
