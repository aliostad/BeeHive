using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BeeHive;
using BeeHive.Demo.PrismoEcommerce.Entities;
using BeeHive.Demo.PrismoEcommerce.Events;
using BeeHive.Demo.PrismoEcommerce.Repositories;

namespace BeeHive.Demo.PrismoEcommerce.Actors
{

    [ActorDescription("OrderInventoryCheckCompleted-Fulfilment")] 
    public class FulfilmentActor : IProcessorActor
    {

        public FulfilmentActor(ICollectionRepository<Order> orderRepository,
            ICollectionRepository<Shipment> shipmentRepository)
        {
            _shipmentRepository = shipmentRepository;
            _orderRepository = orderRepository;
        }

        private Random _random = new Random();
        private ICollectionRepository<Order> _orderRepository;
        private ICollectionRepository<Shipment> _shipmentRepository;


        public void Dispose()
        {
            
        }

        /// <summary>
        /// A simplistic implementation. In reality fulfilment can be an external system/domain
        /// Here we publish
        /// </summary>
        /// <param name="evnt"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var orderInventoryCheckCompleted = evnt.GetBody<OrderInventoryCheckCompleted>();
            var order = await _orderRepository.GetAsync(orderInventoryCheckCompleted.OrderId);
            if (order.IsCancelled)
            {
                return new Event[0];
            }
            else
            {
                await Task.Delay(_random.Next(1000, 5000));

                var shipment = new Shipment()
                {
                    OrderId = order.Id,
                    Id = Guid.NewGuid(),
                    Address = order.ShippingAddress,
                    DeliveryExpectedDate = DateTime.Now.AddDays(_random.Next(1,5))
                };
                await _shipmentRepository.InsertAsync(shipment);

                return new[]
                {
                    new Event(new OrderShipped()
                    {
                        OrderId = order.Id,
                        ShipmentId = shipment.Id
                    })
                    {
                        QueueName = "OrderShipped",
                        EventType = "OrderShipped"
                    } 
                };
            }
        }
    }
}
