using System;
using Microsoft.ServiceBus.Messaging;

namespace BeeHive
{
    public class ServiceBusEvent : Event
    {
        public ServiceBusEvent(BrokeredMessage message)
        {
            this.Body = message.GetBody<string>();
            this.ContentType = message.ContentType;
            this.Id = message.MessageId;
            this.Timestamp = DateTimeOffset.Parse((string) 
                message.Properties[EventProperties.Timestamp]);
        }
    }
}
