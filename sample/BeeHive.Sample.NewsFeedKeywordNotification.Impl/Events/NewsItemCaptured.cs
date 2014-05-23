using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Sample.NewsFeedKeywordNotification.Impl.Entities;

namespace BeeHive.Sample.NewsFeedKeywordNotification.Impl.Events
{
    public class NewsItemCaptured
    {
        public FeedItem Item { get; set; }
    }
}
