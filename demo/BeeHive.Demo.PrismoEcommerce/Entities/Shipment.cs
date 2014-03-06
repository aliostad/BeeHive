using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;

namespace BeeHive.Demo.PrismoEcommerce.Entities
{
    public class Shipment : IHaveIdentity
    {
        public Guid OrderId { get; set; }

        public Guid Id { get; set; }

        public string Address { get; set; }

        public DateTime DeliveryExpectedDate { get; set; }

    }
}
