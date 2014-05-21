using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Sample.NewsFeedKeywordNotification.Impl.Events
{
    public class NewsItemContainingKeywordIentified
    {
        public SyndicationItem Item { get; set; }

        public string Keyword { get; set; }
    }
}
