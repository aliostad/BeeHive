using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Demo.PrismoEcommerce.Events
{
    public class ProductOutOfStock
    {
        public string ProductId { get; set; }

        public int Quantity { get; set; }
    }
}
