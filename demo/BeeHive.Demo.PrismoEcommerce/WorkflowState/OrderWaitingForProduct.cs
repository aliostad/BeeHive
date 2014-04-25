using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;

namespace BeeHive.Demo.PrismoEcommerce.WorkflowState
{
    public class OrderWaitingForProduct : IHaveIdentity
    {

       

        public string OrderId { get; set; }

        public int Quantity { get; set; }

        public string Id { get { return OrderId; } }
    }
}
