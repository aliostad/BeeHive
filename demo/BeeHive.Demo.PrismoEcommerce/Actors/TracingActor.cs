using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Demo.PrismoEcommerce.Actors
{

    [ActorDescription("PaymentAuthorised-Tracing")]
    [ActorDescription("FrauCheckFailed-Tracing")]
    [ActorDescription("OrderInventoryCheckCompleted-Tracing")]
    [ActorDescription("ItemBackInStockForOrder-Tracing")]
    [ActorDescription("OrderShipped-Tracing")]
    [ActorDescription("OrderAccepted-Tracing")]
    [ActorDescription("PaymentFailed-Tracing")]
    [ActorDescription("ProductArrivedBackInStock-Tracing")]
    [ActorDescription("ProductOutOfStock-Tracing")]
    public class TracingActor : IProcessorActor
    {
        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            Trace.TraceInformation("{0}: {1}", evnt.EventType, evnt.Body);
            return new Event[0];
        }
    }
}
