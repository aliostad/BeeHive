using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ActorDescriptionAttribute : Attribute
    {
        private readonly string _name;
        private readonly int _degreeOfParallelism;
        private readonly int _maxIntervalSeconds;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">
        /// Queue name containing the messages that the actor can process messages of.
        /// It can be in the format of [queueName] or [topicName]-[subscriptionName]
        /// </param>
        public ActorDescriptionAttribute(string name, 
            int degreeOfParallelism = 1, 
            int maxIntervalSeconds = 30)
        {
            _maxIntervalSeconds = maxIntervalSeconds;
            _degreeOfParallelism = degreeOfParallelism;
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public int DegreeOfParallelism
        {
            get { return _degreeOfParallelism; }
        }

        public int MaxIntervalSeconds
        {
            get { return _maxIntervalSeconds; }
        }
    }
}
