using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeeHive.Scheduling
{
    public class AsyncPoller : IService
    {
        private readonly IInterval _interval;
        private Func<CancellationToken, Task<bool>> _work;
        private bool _isWorking = false;
        private CancellationTokenSource _cancellationTokenSource;

        public AsyncPoller(IInterval interval, Func<CancellationToken, Task<bool>> work)
        {
            _work = work;
            _interval = interval;
        }

        /// <summary>
        /// Note: RunAsync and Start ping-pong between each other.
        /// </summary>
        /// <returns></returns>

        private async Task RunAsync()
        {
            bool result = false;
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                result = await _work(_cancellationTokenSource.Token);

            }
            catch (Exception exception)
            {
                Trace.TraceWarning(exception.ToString());
            }
    
            if (result)
                _interval.Reset();
            else
               Thread.Sleep(_interval.Next());
           
            if(_isWorking)
                Start();
        }

        public void Start()
        {
            _isWorking = true;
           Task.Factory.StartNew( ()  => RunAsync());
        }

        public void Stop()
        {
            _isWorking = false;
            _cancellationTokenSource.Cancel();
        }

    }
}
