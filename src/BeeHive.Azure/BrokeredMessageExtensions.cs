using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace BeeHive.Azure
{
    public static class BrokeredMessageExtensions
    {
        /// <summary>
        /// Creates an event.
        /// 
        /// NOTE: Assumes the payload is a JSON string!!!
        /// </summary>
        /// <param name="message">message with a JSON string payload</param>
        /// <param name="eventType">event type. Normally the topic queue name</param>
        /// <param name="queueName">Fulle queue name</param>
        /// <returns>event</returns>
        public static Event ToEvent(this Message message, 
            string eventType,
            string queueName)
        {
            return
                new Event()
                {
                    Body = Encoding.UTF8.GetString(message.Body),
                    ContentType = message.ContentType,
                    EventType = eventType,
                    QueueName = queueName,
                    UnderlyingMessage = message
                };
        }

        public static Event ToEvent(this Message message,
           QueueName queueName)
            {
            return
                new Event()
                {
                    Body = Encoding.UTF8.GetString(message.Body), 
                    ContentType = message.ContentType,
                    EventType = queueName.TopicName,
                    QueueName = queueName.ToString(),
                    UnderlyingMessage = message,
                    Timestamp = message.SystemProperties.EnqueuedTimeUtc
                };
        }

        public static Message ToMessage(this Event @event)
        {
            var msg = new Message(Encoding.UTF8.GetBytes(@event.Body))
                          {
                              ContentType = @event.ContentType,
                              MessageId = @event.Id
                          };

            if (@event.Properties != null)
            {
                foreach (var property in @event.Properties)
                {
                    msg.UserProperties.Add(property);
                }                
            }

            if (@event.EnqueueTime.HasValue)
            {
                msg.ScheduledEnqueueTimeUtc = @event.EnqueueTime.Value.UtcDateTime;
            }
            
            return msg;
        }


    }
}
