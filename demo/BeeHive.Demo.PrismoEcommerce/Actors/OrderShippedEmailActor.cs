using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Demo.PrismoEcommerce.Entities;
using BeeHive.Demo.PrismoEcommerce.Events;
using BeeHive.Demo.PrismoEcommerce.ExternalSystems;
using BeeHive.Demo.PrismoEcommerce.Repositories;

namespace BeeHive.Demo.PrismoEcommerce.Actors
{

    [ActorDescription("OrderShipped-Email")]
    public class OrderShippedEmailActor : IProcessorActor
    {
        private ICollectionRepository<Order> _orderRepository;
        private ICollectionRepository<Customer> _customerRepository;
        private Emailer _emailer;
        private ICollectionRepository<Shipment> _shipmentRepository;

        public OrderShippedEmailActor(ICollectionRepository<Order> orderRepository,
            ICollectionRepository<Customer> customerRepository,
            ICollectionRepository<Shipment> shipmentRepository,
            Emailer emailer)
        {
            _shipmentRepository = shipmentRepository;
            _emailer = emailer;
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
        }

        public void Dispose()
        {
            
        }


        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var orderShipped = evnt.GetBody<OrderShipped>();
            var order = await _orderRepository.GetAsync(orderShipped.OrderId);
            var shipment = await _shipmentRepository.GetAsync(orderShipped.ShipmentId);
            var customer = await _customerRepository.GetAsync(order.CustomerId);

            _emailer.Send(
                customer.Email,
                string.Format("Hey {0}, your order has been shipped and expected to arrive on {1}",
                customer.Name, shipment.DeliveryExpectedDate.ToLongDateString()));

            return new Event[0];
        }
    }
}
