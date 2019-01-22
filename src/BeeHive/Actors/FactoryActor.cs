using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BeeHive.Events;
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

        // only for non-event driven
        private async Task<bool> Process(CancellationToken cancellationToken)
        {
            if (_queueOperator.IsEventDriven)
                throw new InvalidOperationException("Operator is event driven but used for NextAsync");

            var result = await _queueOperator.NextAsync(
                new QueueName(_actorDescriptor.SourceQueueName));

            var cancellationTokenSource = new CancellationTokenSource();

            if (result.IsSuccessful)
            {
                TheTrace.TraceInformation("Received a message. Id: {0} Queue: {1} ", result.PollingResult.Id, _actorDescriptor.SourceQueueName);
                var actor = (IProcessorActor)_serviceLocator.GetService(_actorDescriptor.ActorType);
                try
                {

                    // this is NOT supposed to be awaited upon!!
                    _queueOperator.KeepExtendingLeaseAsync(result.PollingResult, TimeSpan.FromSeconds(30),
                        cancellationTokenSource.Token).SafeObserve();

                    await ProcessEvent(actor, result.PollingResult, _queueOperator);

                    await _queueOperator.CommitAsync(result.PollingResult);

                    TheTrace.TraceInformation("Processing succeeded. Id: {0} Queue: {1} " , result.PollingResult.Id, _actorDescriptor.SourceQueueName);
                }
                catch (Exception exception)
                {
                    TheTrace.TraceInformation("Processing failed. Id: {0} Queue: {1} ", result.PollingResult.Id, _actorDescriptor.SourceQueueName);
                    TheTrace.TraceError(exception.ToString());
                    cancellationTokenSource.Cancel();
                    _queueOperator.AbandonAsync(result.PollingResult).SafeObserve().Wait();

                }
                finally
                {                    
                   if (result.IsSuccessful)
                       TryDisposeMessage(result.PollingResult);
                    _serviceLocator.ReleaseService(actor);
                }
            }

            return result.IsSuccessful;
        }

        internal async static Task ProcessEvent(IProcessorActor actor, Event ev, IEventQueueOperator queueOperator)
        {
            var events = (await actor.ProcessAsync(ev)).ToArray(); // the enumerable has to be turned into a list anyway. it does further on
            var groups = events.GroupBy(x => x.QueueName);

            foreach (var gr in groups)
            {
                await queueOperator.PushBatchAsync(gr);
                TryDisposeMessages(gr);
            }
        }


        private static void TryDisposeMessages(IEnumerable<Event> es)
        {
            foreach (var e in es)
            {
                e.TryDisposeUnderlying();
            }
        }

        private static void TryDisposeMessage(Event e)
        {
            e.TryDisposeUnderlying();
        }

        public void Start()
        {
            if (_poller != null)
                _poller.Start();
            else if(_queueOperator.IsEventDriven)
            {
                _queueOperator.RegisterHandler(async (e, oper) =>
               {
                   var actor = (IProcessorActor)_serviceLocator.GetService(_actorDescriptor.ActorType);
                   await ProcessEvent(actor, e, oper);
               }, _actorDescriptor);
            }
            else
            {
                throw new InvalidOperationException("You need to call setup first!");
            }
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
            if (!_queueOperator.IsEventDriven)
            {
                _poller = new AsyncPoller(descriptor.Interval, (Func<CancellationToken, Task<bool>>)Process);
            }
        }
    }
}
