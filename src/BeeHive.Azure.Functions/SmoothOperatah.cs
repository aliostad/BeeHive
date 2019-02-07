using BeeHive.Actors;
using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Azure.Functions
{
    /// <summary>
    /// Smoothly directs your "calls" to the actors
    /// </summary>
    public static class SmoothOperatah
    {
        public static Task Invoke(this IProcessorActor actor, QueueName queueName, Message msg, IEventQueueOperator queueOperator)
        {
            var ev = msg.ToEvent(queueName);
            return FactoryActor.ProcessEvent(actor, ev, queueOperator);
        }
    }
}
