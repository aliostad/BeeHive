using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using BeeHive.Demo.PrismoEcommerce.Events;

namespace BeeHive.Demo.PrismoEcommerce.Actors
{

    [ActorDescription("ProductOutOfStock-ReplenishStock")]
    public class ProductOutOfStockActor : IProcessorActor
    {
        private ICounterStore _stockLevelStore;

        public ProductOutOfStockActor(ICounterStore stockLevelStore)
        {
            _stockLevelStore = stockLevelStore;
        }


        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var productOutOfStock = evnt.GetBody<ProductOutOfStock>();

            Trace.TraceInformation("ProductOutOfStockActor - received");

            await Task.Delay(10*1000);

            Trace.TraceInformation("ProductOutOfStockActor - waiting to get orders from suppliers now back");

            await _stockLevelStore.IncrementAsync(productOutOfStock.ProductId,
                productOutOfStock.Quantity + 2);

            return new[] { new Event(new ProductArrivedBackInStock()
            {
                ProductId = productOutOfStock.ProductId
            })
            {
                QueueName = "ProductArrivedBackInStock",
                EventType = "ProductArrivedBackInStock"
            }
            
            };

        }
    }
}
