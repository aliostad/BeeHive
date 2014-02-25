using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Demo.PrismoEcommerce.Events
{
    public class ItemBackInStockForOrder
    {

        public Guid OrderId { get; set; }

        public Guid ProductId { get; set; }
    }
}
