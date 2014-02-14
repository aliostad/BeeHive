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
    [ActorDescription("FrauCheckFailed-CancelOrder")]
    public class FraudCancelOrderActor : IProcessorActor
    {
        private ICollectionRepository<Order> _orderRepository;

        public FraudCancelOrderActor(ICollectionRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var fraudCheckFailed = evnt.GetBody<FraudCheckFailed>();
            var order = await _orderRepository.GetAsync(fraudCheckFailed.OrderId);

            if (order.IsCancelled)
                return new Event[0];

            order.IsCancelled = true;
            await _orderRepository.UpsertAsync(order);
            return new[]
            {
                new Event(new OrderCancelled()
                {
                    OrderId = order.Id,
                    Reason = "Fraudulent payment"
                })
                {
                    EventType = "OrderCancelled",
                    QueueName = "OrderCancelled"
                }
            };
        }
    }
}
