using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using BeeHive.Demo.PrismoEcommerce.Entities;
using BeeHive.Demo.PrismoEcommerce.Events;

namespace BeeHive.Demo.PrismoEcommerce.Actors
{
    [ActorDescription("FrauCheckFailed-CancelOrder")]
    public class FraudCancelOrderActor : IProcessorActor
    {
        private ICollectionStore<Order> _orderStore;

        public FraudCancelOrderActor(ICollectionStore<Order> orderStore)
        {
            _orderStore = orderStore;
        }

        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var fraudCheckFailed = evnt.GetBody<FraudCheckFailed>();
            var order = await _orderStore.GetAsync(fraudCheckFailed.OrderId);

            if (order.IsCancelled)
                return new Event[0];

            order.IsCancelled = true;
            await _orderStore.UpsertAsync(order.Id, order);
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
