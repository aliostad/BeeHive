using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;

namespace BeeHive.Demo.PrismoEcommerce.WorkflowState
{
    public class ParkedOrderItem : IHaveIdentity, IConcurrencyAware
    {
        public Guid ProductId { get; set; }

        public bool IsItemReadyToShip { get; set; }

        public Guid Id { get { return ProductId; }}

        public DateTimeOffset? LastModofied { get; set; }
        
        public string ETag { get; set; }
    }
}
