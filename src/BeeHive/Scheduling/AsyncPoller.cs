﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeeHive.Scheduling
{

    // NOTE: Ideally IPulser interface must be on a subtype of 
    // AsyncPoller and AsynPoller free of any BeeHive specific stuff like Event (because it can)
    // but due to restrictions in constructors, this cannot be a subtype.
    public class AsyncPoller :  IPulser
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
        /// If you want to use it as a pulser.
        /// Use this constructor to raise events
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="work"></param>
        public AsyncPoller(IInterval interval, Func<CancellationToken, Task<IEnumerable<Event>>> work)
        {

            _work = async (token) =>
            {
                var events = await work(token);
                var any = events.Any();
                PulseGenerated(this, events);
                return false;
            };
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
               TheTrace.TraceWarning(exception.ToString());
            }

            if (result)
                _interval.Reset();
            else
                await Task.Delay(_interval.Next());
           
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

        public event EventHandler<IEnumerable<Event>> PulseGenerated;
    }
}
