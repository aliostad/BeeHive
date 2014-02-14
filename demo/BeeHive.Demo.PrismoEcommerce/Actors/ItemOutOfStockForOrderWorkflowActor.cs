using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Demo.PrismoEcommerce.Events;
using BeeHive.Demo.PrismoEcommerce.Repositories;

namespace BeeHive.Demo.PrismoEcommerce.Actors
{

    [ActorDescription("ItemOutOfStockForOrder")]
    public class ItemOutOfStockForOrderWorkflowActor : IProcessorActor
    {
        private ICounterRepository _counterRepository;
        private ISimpleKeyedListRepository _productOrdeRepository;

        public ItemOutOfStockForOrderWorkflowActor(ISimpleKeyedListRepository productOrdeRepository,
            ICounterRepository counterRepository)
        {
            _productOrdeRepository = productOrdeRepository;
            _counterRepository = counterRepository;
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

                await _productOrdeRepository.AddAsync(
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

                await _productOrdeRepository.AddAsync(
                    outOfStockForOrder.ProductId,
                    outOfStockForOrder.OrderId
                    );

            }

            return new Event[0];

        }
    }
}
