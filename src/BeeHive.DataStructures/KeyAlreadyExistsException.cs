using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{
    public class KeyAlreadyExistsException : Exception
    {
        public KeyAlreadyExistsException(Guid key)
            : base("Key exists " + key)
        {
            
        }
    }
}
