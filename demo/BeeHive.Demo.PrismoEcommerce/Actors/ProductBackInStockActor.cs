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
    [ActorDescription("ProductArrivedBackInStock")]
    public class ProductBackInStockActor : IProcessorActor
    {
        private IKeyedListStore<OrderWaitingForProduct> _ordersWaitingProvider;
        private ProductArrivedBackInStock _productArrivedBackInStock;


        public ProductBackInStockActor(IKeyedListStore<OrderWaitingForProduct> ordersWaitingProvider)
        {
            _ordersWaitingProvider = ordersWaitingProvider;
        }

        public void Dispose()
        {
            _ordersWaitingProvider.RemoveAsync("OrderQueuedForProduct",
                _productArrivedBackInStock.ProductId).Wait();
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            _productArrivedBackInStock = evnt.GetBody<ProductArrivedBackInStock>();
            var orders = await _ordersWaitingProvider.GetAsync("OrderQueuedForProduct", 
                _productArrivedBackInStock.ProductId);            

            return orders.Select(x => new Event(new ItemBackInStockForOrder()
            {
                ProductId = _productArrivedBackInStock.ProductId,
                OrderId = x.OrderId
            })
            {
                EventType = "ItemBackInStockForOrder",
                QueueName = "ItemBackInStockForOrder"
            });
        }
    }
}
