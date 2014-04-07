using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;

namespace BeeHive.Azure
{
    public interface IPartitionedEntity : IHaveIdentity
    {
        string PartitionName { get; set; }
    }
}
