using System;
using Newtonsoft.Json;

namespace BeeHive
{
    [Serializable]
    public class Event : ICloneable
    {

        private const string ContentTypeFormat = "application/{0}+json";

        public Event(object body)
        {
            if(body==null)
                throw new ArgumentNullException("body");

            ContentType = string.Format(ContentTypeFormat, body.GetType().Name);
            Body = JsonConvert.SerializeObject(body);
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
        public string Id { get; set; }

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
        /// Type of the event
        /// </summary>
        public string EventType { get; set; }


        /// <summary>
        /// Underlying queue message (e.g. BrokeredMessage in case of Azure)
        /// </summary>
        public object UnderlyingMessage { get; set; }


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
