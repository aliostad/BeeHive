using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using BeeHive.Demo.PrismoEcommerce.Entities;
using BeeHive.Demo.PrismoEcommerce.Events;

namespace BeeHive.Demo.PrismoEcommerce.Actors
{
    [ActorDescription("PaymentFailed-CancelOrder")]
    public class PaymenyFailedActor : IProcessorActor
    {
        private ICollectionStore<Order> _orderStore;

        public PaymenyFailedActor(ICollectionStore<Order> orderStore)
        {
            _orderStore = orderStore;
        }

        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var paymentFailed = evnt.GetBody<PaymentFailed>();
            var order =  await _orderStore.GetAsync(paymentFailed.OrderId);
            order.IsCancelled = true;
            await _orderStore.UpsertAsync(order);
            Trace.TraceInformation("Order cancelled");

            return new[]
            {
                new Event(new OrderCancelled()
                {
                    OrderId = order.Id,
                    Reason = "Payment failure"
                })
            };
        }
    }
}
