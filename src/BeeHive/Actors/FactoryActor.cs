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
        private ActorDescriptor _actorDescriptor;
        private readonly IEventQueueOperator _queueOperator;
        private CancellationTokenSource _cancellationTokenSource;
        private AsyncPoller _poller;

        public FactoryActor(IServiceLocator serviceLocator, 
            IEventQueueOperator queueOperator)
        {
            _queueOperator = queueOperator;
            _serviceLocator = serviceLocator;

        }

        private async Task<bool> Process(CancellationToken cancellationToken)
        {
            var result = await _queueOperator.NextAsync(
                new QueueName(_actorDescriptor.SourceQueueName));
            if (result.IsSuccessful)
            {
                var actor = (IProcessorActor)_serviceLocator.GetService(_actorDescriptor.ActorType);
                try
                {
                    var events = (await actor.ProcessAsync(result.PollingResult)).ToArray();
                    await _queueOperator.CommitAsync(result.PollingResult);

                    var groups = events.GroupBy(x=>x.QueueName);

                    foreach (var gr in groups)
                    {
                        await _queueOperator.PushBatchAsync(gr);
                    }

                }
                catch (Exception exception)
                {
                    Trace.TraceWarning(exception.ToString());
                    _queueOperator.AbandonAsync(result.PollingResult).SafeObserve();

                }
                finally
                {
                    _serviceLocator.ReleaseService(actor);
                }
            }

            return result.IsSuccessful;
        }

        public void Start()
        {
            if (_poller == null)
                throw new InvalidOperationException("You need to call Setup first.");

            _poller.Start();
        }

        public void Stop()
        {
            if(_poller==null)
                throw new InvalidOperationException("You need to call Setup first.");

            if(_cancellationTokenSource!=null)
                _cancellationTokenSource.Cancel();
            _poller.Stop();
        }

        public void Setup(ActorDescriptor descriptor)
        {
            if(_actorDescriptor!=null)
                throw new InvalidOperationException("Cannot call Setup twice.");

            _actorDescriptor = descriptor;
            _poller = new AsyncPoller(descriptor.Interval, (Func<CancellationToken, Task<bool>>)Process);
        }
    }
}
