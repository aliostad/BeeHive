using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Demo.PrismoEcommerce.Events
{
    public class OrderItemsNotYetAccountedFor
    {

        public string OrderId { get; set; }

        public Dictionary<string, int> ProductQuantities { get; set; }

        public bool AnyOutOfStock { get; set; }
    }
}
