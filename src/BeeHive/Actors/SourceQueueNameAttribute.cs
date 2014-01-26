using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Actors
{
    public class SourceQueueNameAttribute : Attribute
    {
        private readonly string _name;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">
        /// Queue name containing the messages that the actor can process messages of.
        /// It can be in the format of [queueName] or [topicName]-[subscriptionName]
        /// </param>
        public SourceQueueNameAttribute(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
