using System;
using Newtonsoft.Json;

namespace BeeHive
{
    [Serializable]
    public sealed class Event : ICloneable
    {

        public static readonly Event Empty = new Event();

        private const string ContentTypeFormat = "application/{0}+json";

        public Event(object body)
            : this(body, true)
        {
            
        }
        public Event(object body, bool forTopic)
            : this()
        {
            if(body==null)
                throw new ArgumentNullException("body");

            ContentType = string.Format(ContentTypeFormat, body.GetType().Name);
            Body = JsonConvert.SerializeObject(body);

            // set default event type and queue name
            QueueName = forTopic
                ? BeeHive.QueueName.FromTopicName(body.GetType().Name).ToString()
                : BeeHive.QueueName.FromSimpleQueueName(body.GetType().Name).ToString();
            EventType = body.GetType().Name;

        }



        public Event()
        {
            Id = Guid.NewGuid().ToString("N");
            Timestamp = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Mrks when the event happened. Normally a UTC datetime.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Normally a GUID
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Optional URL to the body of the message if Body can be retrieved 
        /// from this URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Content-Type of the Body. Usually a Mime-Type
        /// Typically body is a serialised JSON and content type is application/[.NET Type]+json
        /// </summary>
        public string ContentType { get; set; }
        
        /// <summary>
        /// String content of the body.
        /// Typically a serialised JSON
        /// </summary>
        public string Body { get; set; }


        /// <summary>
        /// Type of the event. This must be set at the time of creation of event before PushAsync
        /// </summary>
        public string EventType { get; set; }


        /// <summary>
        /// Underlying queue message (e.g. BrokeredMessage in case of Azure)
        /// </summary>
        public object UnderlyingMessage { get; set; }

        /// <summary>
        /// This MUST be set by the Queue Operator upon Creation of message usually in NextAsync!!
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// An optional value that can be used to defer a message. If it is in the past, it is ignored
        /// </summary>
        public DateTimeOffset? EnqueueTime { get; set; }

        public T GetBody<T>()
        {
            if(string.IsNullOrEmpty(Body))
                throw new ArgumentNullException("Body");

            return JsonConvert.DeserializeObject<T>(Body);
        }

        public object Clone()
        {
            return new Event()
            {
                Body = Body,
                ContentType = ContentType,
                EventType = EventType,
                Timestamp = Timestamp,
                UnderlyingMessage = UnderlyingMessage,
                Url = Url
            };
        }
    }
}
