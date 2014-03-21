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


            var inventory = await _inventoryStore.GetAsync(Constants.InventoryCounterName, productId);
            var events = new List<Event>();

            // remove item since it will be dealt with
            notYetAccountedFor.ProductQuantities.Remove(keyValue.Key);

          
            // if out of stock, raise out of stock
            if (quantity > inventory)
            {
                Trace.TraceInformation("OrderItemInventoryActor - Item out of stock");
                
                events.Add(new Event(new ProductOutOfStock()
                {
                    ProductId = productId,
                    Quantity = keyValue.Value
                }));

                events.Add(new Event(new ItemOutOfStockForOrder()
                {
                    OrderId = notYetAccountedFor.OrderId,
                    ProductId = productId,
                    Quantity = quantity
                }));

                notYetAccountedFor.AnyOutOfStock = true;
            }
            else 
            {
                // decrement repo
                await _inventoryStore.IncrementAsync(Constants.InventoryCounterName,
                    productId, -quantity);
                Trace.TraceInformation("OrderItemInventoryActor - Item in stock");
            }

            if (notYetAccountedFor.ProductQuantities.Count > 0)
            {
                // put back in the queue
                events.Add(new Event(new OrderItemsNotYetAccountedFor()
                {
                    OrderId = notYetAccountedFor.OrderId,
                    AnyOutOfStock = notYetAccountedFor.AnyOutOfStock,
                    ProductQuantities = notYetAccountedFor.ProductQuantities
                }));
            }


            if (notYetAccountedFor.ProductQuantities.Count == 0)
            {
                if (notYetAccountedFor.AnyOutOfStock)
                {
                    // TODO: cant remember what needs to be done ;)
                }
                else
                {
                    events.Add(new Event(new OrderInventoryCheckCompleted()
                    {
                        OrderId = notYetAccountedFor.OrderId
                    }));     
                }
                           
            }

            return events;
        }
    }
}

