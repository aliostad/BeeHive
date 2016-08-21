using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{
    public class LockToken
    {

        public LockToken(string resourceId)
        {
            ResourceId = resourceId;
            TokenId = Guid.NewGuid();
            RenewCancellation = new CancellationTokenSource(TimeSpan.FromMinutes(30)); // this is tha MAX it can go so if it crashed somehow, it gets unlockied
        }

        /// <summary>
        /// Every token has an Id
        /// </summary>
        public Guid TokenId { get; private set; }
        
        /// <summary>
        /// Logical resource id. Should not contain other than alphanumeric and - and _
        /// </summary>
        public string ResourceId { get; private set; }

        public CancellationTokenSource RenewCancellation { get; set; }
    }
}
