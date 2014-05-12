using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Scheduling
{
    public class PulserPublisher
    {
        private IEventQueueOperator _queueOperator;

        public PulserPublisher(IEventQueueOperator queueOperator, params IPulser[] pulsers)
        {
            _queueOperator = queueOperator;
            foreach (var pulser in pulsers)
            {
                pulser.PulseGenerated +=pulser_PulseGenerated;
            }
        }

        private async void pulser_PulseGenerated(object sender, IEnumerable<Event> e)
        {
            foreach (var ev in e)
            {
                await _queueOperator.PushAsync(ev);
            }
        }
    }
}
