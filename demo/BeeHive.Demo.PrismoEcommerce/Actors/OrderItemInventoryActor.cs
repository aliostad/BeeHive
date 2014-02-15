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

    [ActorDescription("OrderItemsNotYetAccountedFor")]
    public class OrderItemInventoryActor : IProcessorActor
    {
        private ICounterStore _inventoryStore;

        public OrderItemInventoryActor(ICounterStore inventoryStore)
        {
            _inventoryStore = inventoryStore;
        }

        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var notYetAccountedFor = evnt.GetBody<OrderItemsNotYetAccountedFor>();
            var keyValue = notYetAccountedFor.ProductQuantities.First();
            var productId = keyValue.Key;
            var quantity = keyValue.Value;

            notYetAccountedFor.ProductQuantities.Remove(keyValue.Key);

            var inventory = await _inventoryStore.GetAsync(productId);
            var events = new List<Event>();

            if (quantity > inventory)
            {
                events.Add(new Event(new ItemOutOfStock()
                {
                    ProductId = productId,
                    Quantity = keyValue.Value
                })
                {
                    EventType = "ItemOutOfStock",
                    QueueName = "ItemOutOfStock"
                });


                notYetAccountedFor.AnyOutOfStock = true;
            }
            else
            {
                // decrement repo
                await _inventoryStore.IncrementAsync(productId, -quantity);
            }
            if (notYetAccountedFor.ProductQuantities.Count == 0 &&
                !notYetAccountedFor.AnyOutOfStock)
            {
                events.Add(new Event(new OrderInventoryCheckCompleted()
                {
                    OrderId = notYetAccountedFor.OrderId
                })
                {
                    QueueName = "OrderInventoryCheckCompleted",
                    EventType = "OrderInventoryCheckCompleted"
                });                
            }

            return events;
        }
    }
}

