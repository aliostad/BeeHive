using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using BeeHive.Demo.PrismoEcommerce.Entities;
using BeeHive.Demo.PrismoEcommerce.Events;
using BeeHive.Demo.PrismoEcommerce.ExternalSystems;

namespace BeeHive.Demo.PrismoEcommerce.Actors
{

    [ActorDescription("OrderShipped-Email")]
    public class OrderShippedEmailActor : IProcessorActor
    {
        private ICollectionStore<Order> _orderStore;
        private ICollectionStore<Customer> _customerStore;
        private Emailer _emailer;
        private ICollectionStore<Shipment> _shipmentStore;

        public OrderShippedEmailActor(ICollectionStore<Order> orderStore,
            ICollectionStore<Customer> customerStore,
            ICollectionStore<Shipment> shipmentStore,
            Emailer emailer)
        {
            _shipmentStore = shipmentStore;
            _emailer = emailer;
            _customerStore = customerStore;
            _orderStore = orderStore;
        }

        public void Dispose()
        {
            
        }


        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var orderShipped = evnt.GetBody<OrderShipped>();
            var order = await _orderStore.GetAsync(orderShipped.OrderId);
            var shipment = await _shipmentStore.GetAsync(orderShipped.ShipmentId);
            var customer = await _customerStore.GetAsync(order.CustomerId);

            _emailer.Send(
                customer.Email,
                string.Format("Hey {0}, your order has been shipped and expected to arrive on {1}",
                customer.Name, shipment.DeliveryExpectedDate.ToLongDateString()));

            return new Event[0];
        }
    }
}
