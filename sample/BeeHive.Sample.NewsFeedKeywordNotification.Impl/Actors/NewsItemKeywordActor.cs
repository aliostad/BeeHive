using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Sample.NewsFeedKeywordNotification.Impl.Events;

namespace BeeHive.Sample.NewsFeedKeywordNotification.Impl.Actors
{
    [ActorDescription("NewsItemCaptured-KeywordCheck")]
    public class NewsItemKeywordActor : IProcessorActor
    {

        private const string Keyword = "Ukraine";

        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        { 
            var newsItemCaptured = evnt.GetBody<NewsItemCaptured>();
            if (newsItemCaptured.Item.Title.Text.ToLower()
                .IndexOf(Keyword.ToLower()) >= 0)
            {
                return new Event[]
                {
                    new Event(new NewsItemContainingKeywordIentified()
                    {
                        Item = newsItemCaptured.Item,
                        Keyword = Keyword
                    }), 
                };
            }

            return new Event[0];
        }
    }
}
