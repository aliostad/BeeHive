using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{

    /// <summary>
    /// Value in a key value store which has an id (usually the Path in a cloud storage) 
    /// and metadata and body.
    /// It could optionally implement IConcurrencyAware
    /// </summary>
    public interface IBlob : IHaveIdentity
    {
        Stream Body { get; }

        Dictionary<string, string> Metadata { get; }


        /// <summary>
        /// Blob data in a 
        /// </summary>
        object UnderlyingBlob { get; set; }

    }
}
