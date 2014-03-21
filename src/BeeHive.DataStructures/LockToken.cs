using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{
    public class LockToken
    {

        public LockToken(string resourceId)
        {
            ResourceId = resourceId;
            TokenId = Guid.NewGuid();
        }

        /// <summary>
        /// Every token has an Id
        /// </summary>
        public Guid TokenId { get; private set; }
        
        /// <summary>
        /// Logical resource id. Should not contain other than alphanumeric and - and _
        /// </summary>
        public string ResourceId { get; private set; }
    }
}
