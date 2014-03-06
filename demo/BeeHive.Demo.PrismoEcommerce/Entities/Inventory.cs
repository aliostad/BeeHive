using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;

namespace BeeHive.Demo.PrismoEcommerce.Entities
{


    public class Inventory : IHaveIdentity
    {
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }
        public Guid Id { get { return ProductId; } }
    }
}
