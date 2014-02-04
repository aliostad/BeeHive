using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive
{
    /// <summary>
    /// Processes an event.
    /// 
    /// Queue name containing the messages that the actor can process messages of.
    /// It can be in the format of [queueName] or [topicName]-[subscriptionName]
    /// </summary>
    public interface IProcessorActor : IDisposable
    {
        /// <summary>
        /// Asynchrnous processing of the message
        /// </summary>
        /// <param name="evnt">Event to process</param>
        /// <returns>Typically contains 0-1 messages. Exceptionally more than 1</returns>
        Task<IEnumerable<Event>> ProcessAsync(Event evnt);

    }
}
