using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{
    public class ConcurrencyConflictException : Exception
    {
        public ConcurrencyConflictException(string message)
            : base(message)
        {
            
        }

        public ConcurrencyConflictException(string etag1, string etag2)
            : base(string.Format("Conflict: {0} <-> {1}", etag1, etag2))
        {

        }

        public ConcurrencyConflictException(DateTimeOffset lastModified1, DateTimeOffset lastModified2)
            : base(string.Format("Conflict: {0} <-> {1}", lastModified1, lastModified2))
        {

        }
    }
}
