using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Demo.PrismoEcommerce.Entities;
using BeeHive.Demo.PrismoEcommerce.Events;
using BeeHive.Demo.PrismoEcommerce.Repositories;

namespace BeeHive.Demo.PrismoEcommerce.Actors
{

    [ActorDescription("PaymentAuthorised-InventoryDecrement")]
    public class OrderInventoryActor : IProcessorActor
    {
        private ICollectionRepository<Order> _orderRepository;

        public OrderInventoryActor(ICollectionRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var paymentAuthorised = evnt.GetBody<PaymentAuthorised>();
            var order = await _orderRepository.GetAsync(paymentAuthorised.OrderId);
            
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
