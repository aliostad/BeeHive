using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Demo.PrismoEcommerce.Events
{
    public class FraudCheckFailed
    {
        public string OrderId { get; set; }

        public string PaymentId { get; set; }
    }
}
