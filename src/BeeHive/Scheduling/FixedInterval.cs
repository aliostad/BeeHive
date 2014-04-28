using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Scheduling
{
    public class FixedInterval : IInterval
    {
        private TimeSpan _interval;

        public FixedInterval(TimeSpan interval)
        {
            _interval = interval;
        }

        public TimeSpan Next()
        {
            return _interval;
        }

        public TimeSpan Reset()
        {
            return _interval;
        }
    }
}
