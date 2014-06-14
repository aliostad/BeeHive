using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Scheduling
{
    public class SimpleAutoPulser : IPulser
    {
        private Poller _poller;

        public SimpleAutoPulser(TimeSpan interval, params string[] eventTypes)
        {
            if(eventTypes.Length==0)
                throw new ArgumentOutOfRangeException("eventTypes", "At least one event type needs to be passed in.");
            _poller = new Poller(new FixedInterval(interval), () =>
            {
                if (PulseGenerated != null)
                {
                    PulseGenerated(this, eventTypes.Select(x => new Event(string.Empty)
                    {
                        EventType = QueueName.FromTopicName(x).ToString(),
                        QueueName = QueueName.FromTopicName(x).ToString()
                    }).ToArray());
                }
                    
                return false;
            });
        }

        public event EventHandler<IEnumerable<Event>> PulseGenerated;

        public void Start()
        {
           _poller.Start();
        }

        public void Stop()
        {
            _poller.Stop();
        }
    }
}
