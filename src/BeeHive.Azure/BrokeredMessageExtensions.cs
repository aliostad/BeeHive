using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Azure.ServiceBus;

namespace BeeHive.Azure
{
    public static class BrokeredMessageExtensions
    {
        private static readonly DataContractSerializer _oldSerialiser = new DataContractSerializer(typeof(string));

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
            string body = null;
            if (message.Body[0] == Convert.ToChar('{')) // this is from previous version of ServiceBus which serialises
            {
                var ms = new MemoryStream(message.Body);
                var reader = XmlDictionaryReader.CreateBinaryReader(ms, XmlDictionaryReaderQuotas.Max);
                body = (string)_oldSerialiser.ReadObject(reader);
            }
            else
            {
                body = Encoding.UTF8.GetString(message.Body);
            }

            return
                new Event()
                {
                    Body = body,
                    ContentType = message.ContentType,
                    EventType = eventType,
                    QueueName = queueName,
                    UnderlyingMessage = message
                };
        }

        public static Event ToEvent(this Message message,
           QueueName queueName)
        {
            return ToEvent(message, queueName.TopicName, queueName.ToString());
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
