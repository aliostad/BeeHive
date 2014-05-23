using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Sample.NewsFeedKeywordNotification.Impl.Entities;

namespace BeeHive.Sample.NewsFeedKeywordNotification.Impl
{
    internal static class SyndicationItemExtensions
    {
        public static FeedItem ToFeedItem(this SyndicationItem item)
        {
            return new FeedItem()
            {
                Summary = item.Summary.Text,
                Title = item.Title.Text,
                Url = item.Links[0].Uri.ToString()
            };
        }
    }
}
