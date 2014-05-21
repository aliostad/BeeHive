using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using BeeHive.Sample.NewsFeedKeywordNotification.Impl.Events;

namespace BeeHive.Sample.NewsFeedKeywordNotification.Impl.Actors
{
    [ActorDescription("NewsPulse-Disseminate")]
    public class NewsPulseActor : IProcessorActor
    {
        private IKeyValueStore _keyValueStore;
        public NewsPulseActor(IKeyValueStore keyValueStore)
        {
            _keyValueStore = _keyValueStore;
        }
        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var events = new List<Event>();
            var blob = await _keyValueStore.GetAsync("newsFeeds.txt");
            var reader = new StreamReader(blob.Body);

            string line = string.Empty;
            while ((line = reader.ReadLine()) != null)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    events.Add(new Event(new NewsFeedPulsed() { Url = line }));
                }
            }
            return events;
        }

        public void Dispose()
        {
            
        }
    }
}
