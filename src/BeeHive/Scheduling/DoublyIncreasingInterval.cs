using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Scheduling
{
    public class DoublyIncreasingInterval : IntervalBase
    {

        private TimeSpan _increment;


        public DoublyIncreasingInterval(TimeSpan startInterval, TimeSpan maxInterval, int intervalCount) 
            : base(startInterval, maxInterval)
        {
            _increment = new TimeSpan((maxInterval.Ticks - startInterval.Ticks) / intervalCount);

        }

        protected override TimeSpan CalculateNext(TimeSpan current)
        {
            return current + _increment;
        }
    }
}
