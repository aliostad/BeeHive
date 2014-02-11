using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Demo.PrismoEcommerce.Entities;
using BeeHive.Demo.PrismoEcommerce.Events;
using BeeHive.Demo.PrismoEcommerce.Repositories;

namespace BeeHive.Demo.PrismoEcommerce.Actors
{

    [ActorDescription("OrderItemsNotYetAccountedFor")]
    public class OrderItemInventoryActor : IProcessorActor
    {
        private IRepository<Inventory> _inventoryRepository;

        public OrderItemInventoryActor(IRepository<Inventory> inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var notYetAccountedFor = evnt.GetBody<OrderItemsNotYetAccountedFor>();
            var keyValue = notYetAccountedFor.ProductQuantities.First();

            notYetAccountedFor.ProductQuantities.Remove(keyValue.Key);

            var inventory = await _inventoryRepository.GetAsync(keyValue.Key);
            if (keyValue.Value > inventory.Quantity)
            {
                
            }

            throw new NotImplementedException();

        }
    }
}

