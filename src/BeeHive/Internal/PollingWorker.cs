using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeeHive.Internal
{

    [Obsolete("this is classic approach")]
    internal class PollingWorker
    {

        private readonly IInterval _interval;
        private AutoResetEvent _resetEvent;
        private readonly Func<bool> _work;
        private Thread _worker;
        private bool _isWorking = false;
        public PollingWorker(IInterval interval, Func<bool> work)
        {
            _work = work;
            _interval = interval;
        }

        public void Start()
        {
            Stop();
            _isWorking = true;
            _resetEvent = new AutoResetEvent(false);
            _worker = new Thread(Run);
            _worker.Start();
        }

        public void Stop()
        {
            _isWorking = false;
            if (_worker != null)
            {
                _resetEvent.Set();
                _worker.Abort();
            }
        }

        private void Run()
        {
            while (true)
            {
                if(!_isWorking)
                    break;

                if (_work.WrapException(true))
                {
                    _interval.Reset();
                }
                else
                {
                    _resetEvent.WaitOne(_interval.Next());
                }
            }
            
        }

    }
}
