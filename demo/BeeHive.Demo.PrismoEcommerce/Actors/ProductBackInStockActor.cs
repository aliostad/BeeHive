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
    [ActorDescription("ProductArrivedBackInStock-ActionParkedOrders")]
    public class ProductBackInStockActor : IProcessorActor
    {
        private IKeyedListStore<OrderWaitingForProduct> _ordersWaitingProvider;


        public ProductBackInStockActor(IKeyedListStore<OrderWaitingForProduct> ordersWaitingProvider)
        {
            _ordersWaitingProvider = ordersWaitingProvider;
        }

        public void Dispose()
        {

        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var productArrivedBackInStock = evnt.GetBody<ProductArrivedBackInStock>();
            var orders = await _ordersWaitingProvider.GetAsync("OrderQueuedForProduct", 
                productArrivedBackInStock.ProductId);

            _ordersWaitingProvider.RemoveAsync("OrderQueuedForProduct",
                productArrivedBackInStock.ProductId).Wait();


            return orders.Select(x => new Event(new ItemBackInStockForOrder()
            {
                ProductId = productArrivedBackInStock.ProductId,
                OrderId = x.OrderId
            }));
        }
    }
}
