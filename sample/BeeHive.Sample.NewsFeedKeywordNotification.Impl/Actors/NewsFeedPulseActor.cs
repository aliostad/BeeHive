using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BeeHive.DataStructures;
using BeeHive.Sample.NewsFeedKeywordNotification.Impl.Entities;
using BeeHive.Sample.NewsFeedKeywordNotification.Impl.Events;

namespace BeeHive.Sample.NewsFeedKeywordNotification.Impl.Actors
{

    [ActorDescription("NewsFeedPulsed-Capture")]    
    public class NewsFeedPulseActor : IProcessorActor
    {
        private ICollectionStore<FeedChannel> _channelStore;

        public NewsFeedPulseActor(ICollectionStore<FeedChannel> channelStore)
        {
            _channelStore = channelStore;
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var newsFeedPulsed = evnt.GetBody<NewsFeedPulsed>();
            var client = new HttpClient();
            var stream = await client.GetStreamAsync(newsFeedPulsed.Url);
            var feedChannel = new FeedChannel(newsFeedPulsed.Url);
            var feed = SyndicationFeed.Load(XmlReader.Create(stream));
            var offset = DateTimeOffset.MinValue;
            if (await _channelStore.ExistsAsync(feedChannel.Id))
            {
                feedChannel = await _channelStore.GetAsync(feedChannel.Id);
                offset = feedChannel.LastOffset;
            }
            feedChannel.LastOffset = feed.Items.Max(x => x.PublishDate);
             await _channelStore.UpsertAsync(feedChannel);
            return feed.Items.OrderByDescending(x => x.PublishDate)
                .TakeWhile(y => offset < y.PublishDate)
                .Select(z => new Event(new NewsItemCaptured(){Item = z}));

        }

        public void Dispose()
        {

        }
    }
}
