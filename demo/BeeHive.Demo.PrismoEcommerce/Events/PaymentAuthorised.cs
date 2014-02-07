using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Demo.PrismoEcommerce.Events
{
    class PaymentAuthorised
    {
        public Guid OrderId { get; set; }

        public Guid PaymentId { get; set; }
    }
}
