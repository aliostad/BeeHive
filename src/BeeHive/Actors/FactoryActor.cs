using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BeeHive.Internal;
using BeeHive.Scheduling;

namespace BeeHive.Actors
{
    public class FactoryActor : IFactoryActor
    {
        private readonly IServiceLocator _serviceLocator;
        private readonly ActorDescriptor _actorDescriptor;
        private readonly IEventQueueOperator _queueOperator;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly AsyncPoller _poller;

        public FactoryActor(IServiceLocator serviceLocator, 
            ActorDescriptor actorDescriptor,
            IEventQueueOperator queueOperator)
        {
            _queueOperator = queueOperator;
            _actorDescriptor = actorDescriptor;
            _serviceLocator = serviceLocator;
            _poller = new AsyncPoller(actorDescriptor.Interval, Process);

        }

        private async Task<bool> Process(CancellationToken cancellationToken)
        {
            var result = await _queueOperator.NextAsync(_actorDescriptor.SourceQueueName);
            if (result.IsSuccessful)
            {
                var actor = (IProcessorActor)_serviceLocator.GetService(_actorDescriptor.ActorType);
                try
                {
                    var evt = await actor.ProcessAsync(result.PollingResult);
                    await _queueOperator.Commit(result.PollingResult);
                }
                catch (Exception exception)
                {
                    Trace.TraceWarning(exception.ToString());
                    _queueOperator.Abandon(result.PollingResult).SafeObserve();
                    
                }
            }

            return result.IsSuccessful;
        }

        public void Start()
        {
            _poller.Start();
        }

        public void Stop()
        {
            if(_cancellationTokenSource!=null)
                _cancellationTokenSource.Cancel();
            _poller.Stop();
        }
    }
}
