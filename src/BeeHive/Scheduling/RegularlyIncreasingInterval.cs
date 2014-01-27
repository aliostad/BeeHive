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

            _increment = new TimeSpan((maxInterval.Ticks-startInterval.Ticks)/intervaCount);
        }

        protected override TimeSpan CalculateNext(TimeSpan current)
        {
            return current.Add(_increment);
        }
    }
}
