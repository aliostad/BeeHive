using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Demo.PrismoEcommerce.Events
{
    public class OrderProductsAccepted
    {
        public Dictionary<Guid, int> ProductQuantities { get; set; } 
    }
}
