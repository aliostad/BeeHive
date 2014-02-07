using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Demo.PrismoEcommerce.Entities
{
    public class Payment
    {
        public Guid OrderId { get; set; }

        public decimal Amount { get; set; }
    }
}
