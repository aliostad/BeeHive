using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{
    public class SimpleBlob : IBlob, IConcurrencyAware
    {
        public Stream Body { get; set; }

        public Dictionary<string, string> Metadata { get; set; }

        public object UnderlyingBlob { get; set; }

        /// <summary>
        /// This is also the path
        /// </summary>
        public string Id { get; set; }

        public DateTimeOffset? LastModofied { get; set; }

        public string ETag { get; set; }
    }
}
