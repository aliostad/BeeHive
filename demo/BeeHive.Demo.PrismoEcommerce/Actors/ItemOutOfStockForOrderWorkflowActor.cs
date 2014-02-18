using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using BeeHive.Demo.PrismoEcommerce.Events;
using BeeHive.Demo.PrismoEcommerce.WorkflowState;

namespace BeeHive.Demo.PrismoEcommerce.Actors
{

    [ActorDescription("ItemOutOfStockForOrder")]
    public class ItemOutOfStockForOrderWorkflowActor : IProcessorActor
    {
        private ICounterStore _counterStore;
        private IKeyedListStore<ParkedOrderItem> _parkedOrderItemStore;
        private IKeyedListStore<OrderWaitingForProduct> _orderWaitingForProductStore;

        public ItemOutOfStockForOrderWorkflowActor(
            IKeyedListStore<ParkedOrderItem> parkedOrderItemStore,
            IKeyedListStore<OrderWaitingForProduct> orderWaitingForProductStore,
            ICounterStore counterStore)
        {
            _orderWaitingForProductStore = orderWaitingForProductStore;
            _parkedOrderItemStore = parkedOrderItemStore;
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

                await _parkedOrderItemStore.AddAsync("ProductQueuedForOrder",
                    outOfStockForOrder.OrderId,
                    new ParkedOrderItem()
                    {
                        ProductId = outOfStockForOrder.ProductId
                    });
                    

                outOfStockForOrder.ProductQueuedForOrder = true;
                return new[] {new Event( outOfStockForOrder)
                {
                    QueueName = "ItemOutOfStockForOrder",
                    EventType = "ItemOutOfStockForOrder"
                }};
            }

            if (!outOfStockForOrder.OrderQueuedForProduct)
            {

                await _orderWaitingForProductStore.AddAsync("OrderQueuedForProduct",
                    outOfStockForOrder.ProductId,
                    new OrderWaitingForProduct()
                    {
                        OrderId = outOfStockForOrder.OrderId,
                        Quantity = outOfStockForOrder.Quantity
                    });

            }

            return new Event[0];

        }
    }
}
