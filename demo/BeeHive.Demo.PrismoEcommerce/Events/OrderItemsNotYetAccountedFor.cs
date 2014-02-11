using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Demo.PrismoEcommerce.Events
{
    public class OrderItemsNotYetAccountedFor
    {

        public Guid OrderId { get; set; }

        public Dictionary<Guid, int> ProductQuantities { get; set; } 
    }
}
