using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{
    public interface IHaveIdentityAndRange : IHaveIdentity
    {
        string RangeKey { get; set; }
    }
}
