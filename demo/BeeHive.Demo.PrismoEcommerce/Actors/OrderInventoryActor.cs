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

    [ActorDescription("PaymentAuthorised-InventoryDecrement")]
    public class OrderInventoryActor : IProcessorActor
    {
        private ICollectionStore<Order> _orderStore;

        public OrderInventoryActor(ICollectionStore<Order> orderStore)
        {
            _orderStore = orderStore;
        }

        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var paymentAuthorised = evnt.GetBody<PaymentAuthorised>();
            var order = await _orderStore.GetAsync(paymentAuthorised.OrderId);

            Trace.TraceInformation("OrderInventoryActor - ProductRequested");

            var @event = new Event(new OrderItemsNotYetAccountedFor()
            {
                ProductQuantities = order.ProductQuantities,
                OrderId = order.Id
            })
            {
                    QueueName = "OrderItemsNotYetAccountedFor",
                    EventType = "OrderItemsNotYetAccountedFor"
            };
            return new []
            {
                @event
            };
        }
    }
}
