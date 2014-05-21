using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Sample.NewsFeedKeywordNotification.Impl.Events;

namespace BeeHive.Sample.NewsFeedKeywordNotification.Impl.Actors
{

    [ActorDescription("NewsItemContainingKeywordIentified-TraceNotification")]
    public class TraceNotificationActor : IProcessorActor
    {
        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var keywordIentified = evnt.GetBody<NewsItemContainingKeywordIentified>();
            Trace.TraceInformation("Found {0} in {1}",
                keywordIentified.Keyword, keywordIentified.Item.Links[0].Uri);
            return new Event[0];
        }
    }
}
