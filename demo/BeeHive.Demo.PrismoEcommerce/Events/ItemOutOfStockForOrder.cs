using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Demo.PrismoEcommerce.Events
{
    public class ItemOutOfStockForOrder
    {
        public Guid ProductId { get; set; }

        public Guid OrderId { get; set; }

        public int Quantity { get; set; }

        public bool ProductRequested { get; set; }

        public bool OrderQueuedForProduct { get; set; }

        public bool ProductQueuedForOrder { get; set; }

    }
}
