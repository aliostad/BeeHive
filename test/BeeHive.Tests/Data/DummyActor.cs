using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Tests.Data
{

    [ActorDescription("Dummy", 3, 200)]
    public class DummyActor : IProcessorActor
    {
        public void Dispose()
        {
            
        }

        public Task<Event> ProcessAsync(Event evnt)
        {
            throw new NotImplementedException();
        }
    }
}
