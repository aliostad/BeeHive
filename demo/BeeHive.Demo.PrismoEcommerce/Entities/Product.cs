using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;

namespace BeeHive.Demo.PrismoEcommerce.Entities
{
    public class Product : IHaveIdentity
    {

        public Product()
        {
            Id = Guid.NewGuid().ToString("N");            
        }

        public string Id { get; set; }

        public string Name { get; set; }


    }
}
