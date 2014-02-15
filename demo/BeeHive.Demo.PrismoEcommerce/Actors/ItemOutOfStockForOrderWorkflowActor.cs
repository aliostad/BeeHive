using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using BeeHive.Demo.PrismoEcommerce.Events;

namespace BeeHive.Demo.PrismoEcommerce.Actors
{

    [ActorDescription("ItemOutOfStockForOrder")]
    public class ItemOutOfStockForOrderWorkflowActor : IProcessorActor
    {
        private ICounterStore _counterStore;
        private ISimpleKeyedListStore _productOrdeStore;

        public ItemOutOfStockForOrderWorkflowActor(ISimpleKeyedListStore productOrdeStore,
            ICounterStore counterStore)
        {
            _productOrdeStore = productOrdeStore;
            _counterStore = counterStore;
        }

        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var outOfStockForOrder = evnt.GetBody<ItemOutOfStockForOrder>();
            if (!outOfStockForOrder.ProductRequested)
            {
                outOfStockForOrder.ProductRequested = true;
                return new[] {new Event( outOfStockForOrder)
                {
                    QueueName = "ItemOutOfStockForOrder",
                    EventType = "ItemOutOfStockForOrder"
                }};
            }

            if (!outOfStockForOrder.ProductQueuedForOrder)
            {

                await _productOrdeStore.AddAsync(
                    outOfStockForOrder.OrderId,
                    outOfStockForOrder.ProductId);

                outOfStockForOrder.ProductQueuedForOrder = true;
                return new[] {new Event( outOfStockForOrder)
                {
                    QueueName = "ItemOutOfStockForOrder",
                    EventType = "ItemOutOfStockForOrder"
                }};
            }

            if (!outOfStockForOrder.OrderQueuedForProduct)
            {

                await _productOrdeStore.AddAsync(
                    outOfStockForOrder.ProductId,
                    outOfStockForOrder.OrderId
                    );

            }

            return new Event[0];

        }
    }
}
