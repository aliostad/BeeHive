using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using BeeHive.Demo.PrismoEcommerce.Events;
using BeeHive.Demo.PrismoEcommerce.WorkflowState;

namespace BeeHive.Demo.PrismoEcommerce.Actors
{

    [ActorDescription("ItemBackInStockForOrder-ProcessOrder")]
    public class ItemBackInStockForOrderActor : IProcessorActor
    {
        private IKeyedListStore<ParkedOrderItem> _parkedOrderItemsStore;

        public ItemBackInStockForOrderActor(IKeyedListStore<ParkedOrderItem> parkedOrderItemsStore)
        {
            _parkedOrderItemsStore = parkedOrderItemsStore;
        }

        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var itemBackInStockForOrder = evnt.GetBody<ItemBackInStockForOrder>();
            var parkedItems =  (await _parkedOrderItemsStore.GetAsync("ProductQueuedForOrder",
                itemBackInStockForOrder.OrderId)).ToArray();
            var parkedOrderItem = parkedItems.Single(x => x.ProductId == itemBackInStockForOrder.ProductId);
            parkedOrderItem.IsItemReadyToShip = true;
            _parkedOrderItemsStore.UpdateAsync("ProductQueuedForOrder", itemBackInStockForOrder.OrderId,
                parkedOrderItem);
            if (parkedItems.Any(x => !x.IsItemReadyToShip))
            {
                Trace.TraceInformation("ItemBackInStockForOrderActor - still items remaining");
                return new Event [0];
            }
            else // ready
            {
                Trace.TraceInformation("ItemBackInStockForOrderActor - itemss all OK");

                return new[]
                {
                    new Event(new OrderInventoryCheckCompleted()
                    {
                        OrderId = itemBackInStockForOrder.OrderId
                    })
                };
            }

        }
    }
}
