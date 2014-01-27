using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive
{
    public abstract class IntervalBase : IInterval
    {
        protected readonly TimeSpan _startInterval;
        protected readonly TimeSpan _maxInterval;

        private TimeSpan _current;
        protected abstract TimeSpan CalculateNext(TimeSpan current);

        public IntervalBase(TimeSpan startInterval, TimeSpan maxInterval)
        {
            _maxInterval = maxInterval;
            _startInterval = startInterval;
            _current = _startInterval;
        }

        public TimeSpan Next()
        {
            TimeSpan next = CalculateNext(_current);

            if(next<_startInterval)
                throw new InvalidOperationException("Next interval must be equal or greater to start interval.");

            _current = new TimeSpan(Math.Min(_maxInterval.Ticks, next.Ticks));

            return _current;
        }

        public TimeSpan Reset()
        {
            return _current = _startInterval;
        }
    }
}
