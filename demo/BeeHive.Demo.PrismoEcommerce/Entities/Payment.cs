using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;

namespace BeeHive.Demo.PrismoEcommerce.Entities
{
    public class Payment : IHaveIdentity
    {
        public Payment()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        public string Id { get; set; }

        public string OrderId { get; set; }

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; }

        public string TransactionId { get; set; }
    }
}
