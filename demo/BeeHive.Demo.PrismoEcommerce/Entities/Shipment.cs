using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Demo.PrismoEcommerce.Entities
{
    public class Shipment
    {
        public Guid OrderId { get; set; }

        public Guid Id { get; set; }

        public string Address { get; set; }

        public DateTime DeliveryExpectedDate { get; set; }

    }
}
