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

        public string ProductId { get; set; }

        public int Quantity { get; set; }

        public string Id
        {
            get
            {
                return ProductId;
            }
            set { ProductId = value; }
        }
    }
}
