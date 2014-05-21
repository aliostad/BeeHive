using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;

namespace BeeHive.Sample.NewsFeedKeywordNotification.Impl.Entities
{
    public class FeedChannel : IHaveIdentity
    {
        public FeedChannel()
        {
            
        }

        public FeedChannel(string url)
        {
            Id = Convert.ToBase64String(Encoding.UTF8.GetBytes(url))
                .Replace('/', '-');
        }

        public DateTimeOffset LastOffset { get; set; }

        public string Id { get; set; }
    }
}
