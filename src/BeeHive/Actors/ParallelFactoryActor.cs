using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeeHive.Internal;
using BeeHive.Scheduling;

namespace BeeHive.Actors
{
    /// <summary>
    /// Uses a single poller to dispatch events to multiple actors at once.
    /// </summary>
    public class ParallelFactoryActor : IFactoryActor
    {
        private readonly IServiceLocator serviceLocator;
        private ActorDescriptor actorDescriptor;
        private readonly IEventQueueOperator queueOperator;
        private AsyncPoller poller;
        private List<Task> currentTasks = new List<Task>();

        public ParallelFactoryActor(IServiceLocator serviceLocator, 
            IEventQueueOperator queueOperator)
        {
            this.queueOperator = queueOperator;
            this.serviceLocator = serviceLocator;
        }

        public void Start()
        {
            if (poller == null)
            {
                throw new InvalidOperationException("You need to call Setup first.");
            }

            poller.Start();
        }

        public void Stop()
        {
            if (poller == null)
            {
                throw new InvalidOperationException("You need to call Setup first.");
            }

            poller.Stop();
        }

        public void Setup(ActorDescriptor descriptor)
        {
            if (actorDescriptor != null)
            {
                throw new InvalidOperationException("Cannot call Setup twice.");
            }

            actorDescriptor = descriptor;
            poller = new AsyncPoller(descriptor.Interval, (Func<CancellationToken, Task<bool>>)Process);
        }

        private async Task<bool> Process(CancellationToken cancellationToken)
        {
            var result = await queueOperator.NextAsync(new QueueName(actorDescriptor.SourceQueueName));
            if (result.IsSuccessful)
            {
                await Dispatch(result.PollingResult);
            }
            return result.IsSuccessful;
        }

        private async Task Dispatch(Event e)
        {
            while (currentTasks.Count >= actorDescriptor.DegreeOfParallelism)
            {
                await Task.WhenAny(currentTasks);
                currentTasks = currentTasks.Where(x => !x.IsCompleted).ToList();
            }
            currentTasks.Add(Task.Run(async() => await ProcessEvent(e)));
        }

        private async Task ProcessEvent(Event e)
        {
            var actor = (IProcessorActor)serviceLocator.GetService(actorDescriptor.ActorType);
            try
            {
                var events = (await actor.ProcessAsync(e)).ToArray();
                await queueOperator.CommitAsync(e);

                var groups = events.GroupBy(x => x.QueueName);

                foreach (var gr in groups)
                {
                    await queueOperator.PushBatchAsync(gr);
                }

            }
            catch (Exception exception)
            {
                Trace.TraceWarning(exception.ToString());
                queueOperator.AbandonAsync(e).SafeObserve();

            }
            finally
            {
                serviceLocator.ReleaseService(actor);
            }
        }
    }
}
