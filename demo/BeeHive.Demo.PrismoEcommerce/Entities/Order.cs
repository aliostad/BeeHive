using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;

namespace BeeHive.Demo.PrismoEcommerce.Entities
{
    public class Order : IHaveIdentity
    {

        public Order()
        {
            Id = Guid.NewGuid().ToString("N");            
        }

        public string Id { get; set; }

        public string CustomerId { get; set; }

        public Dictionary<string, int> ProductQuantities { get; set; }

        public bool IsCancelled { get; set; }

        public decimal TotalPrice { get; set; }

        public string PaymentMethod { get; set; }

        public string ShippingAddress { get; set; }
    }
}
