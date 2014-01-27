using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Scheduling
{
    public class DoublyIncreasingInterval : IntervalBase
    {
        
        private TimeSpan _firstIncrement;

        public DoublyIncreasingInterval(TimeSpan startInterval, TimeSpan maxInterval, int intervalCount) 
            : base(startInterval, maxInterval)
        {

        }

        protected override TimeSpan CalculateNext(TimeSpan current)
        {
            return current + _firstIncrement;
        }
    }
}
