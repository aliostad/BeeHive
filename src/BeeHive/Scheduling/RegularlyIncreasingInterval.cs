using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive
{
    public class RegularlyIncreasingInterval : IntervalBase
    {
        private TimeSpan _increment;
        public RegularlyIncreasingInterval(TimeSpan startInterval, TimeSpan maxInterval, int intervaCount) 
            : base(startInterval, maxInterval)
        {
            if(intervaCount<=0)
                throw new ArgumentOutOfRangeException("intervaCount");
            var totalTicks = (maxInterval.Ticks-startInterval.Ticks) / (Math.Pow(2,intervaCount)-1);
            _increment = new TimeSpan( (long) totalTicks);
        }

        protected override TimeSpan CalculateNext(TimeSpan current)
        {
            return current.Add(_increment);
        }
    }
}
