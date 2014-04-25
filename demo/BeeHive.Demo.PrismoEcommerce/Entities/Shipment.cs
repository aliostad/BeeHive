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

        public Shipment()
        {
            Id = Guid.NewGuid().ToString("N");            
        }

        public string OrderId { get; set; }

        public string Id { get; set; }

        public string Address { get; set; }

        public DateTime DeliveryExpectedDate { get; set; }

    }
}
