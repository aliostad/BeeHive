using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BeeHive.Internal;

namespace BeeHive.Scheduling
{
    public class Poller
    {
        private readonly IInterval _interval;
        private Func<bool> _work;
        private readonly int _workers;
        private bool _isWorking = false;
        private CancellationTokenSource[] _cancellationTokenSources;

        public Poller(IInterval interval, Func<bool> work, int workers = 1)
        {
            _workers = workers;
            _work = work;
            _interval = interval;
            _cancellationTokenSources = new CancellationTokenSource[workers];
        }

        public void Start()
        {
            _isWorking = true;
            for (int i = 0; i < _workers; i++)
            {
                _cancellationTokenSources[i] = new CancellationTokenSource();
                Task.Factory.StartNew(Run, _cancellationTokenSources[i].Token);
            }
        }

        public void Stop()
        {
            _isWorking = false;
            foreach (var source in _cancellationTokenSources)
            {
                source.Cancel();
            }
        }

        private void Run()
        {
            while (true)
            {
                if (!_isWorking)
                    break;

                if (_work.WrapException(true))
                {
                    _interval.Reset();
                }
                else
                {
                    Thread.Sleep(_interval.Next());
                }
            }

        }

        public IObservable<T> ToObservable<T>(Func<PollerResult<T>> publisher)
        {
            if(_isWorking)
                throw new InvalidOperationException("Cannot be called while started and working. Call when is stopped");

            return Observable.Create((IObserver<T> customer) =>
            {
                _work = () =>
                {
                    var result = publisher();
                    if(result.IsSuccessful)
                        customer.OnNext(result.PollingResult);

                    return result.IsSuccessful;
                };
                return Stop;
            });

        }

    }
}
