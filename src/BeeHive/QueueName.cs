using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BeeHive
{

    /// <summary>
    /// Parses queue name.
    /// Queue name is in the format of [QueueName] for simple queues or [TopicName]-[SubscriptionName] for topics
    /// </summary>
    public class QueueName
    {
        private string _queueName;

        private const string QueueNamePattern = @"^(\w+)(?:\-(\w*))?$";
        public QueueName(string queueName)
        {
            _queueName = queueName;
            var match = Regex.Match(queueName, QueueNamePattern);
            if(!match.Success)
                throw new ArgumentException("Queue name does not follow correct pattern", "queueName");
            TopicName = match.Groups[1].Value;
            SubscriptionName = match.Groups[2].Value;

            if (string.IsNullOrEmpty(SubscriptionName))
                SubscriptionName = TopicName;

        }

        public string TopicName { get; private set; }

        public string SubscriptionName { get; private set; }

        public bool IsSimpleQueue
        {
            get
            {
                return TopicName == SubscriptionName;
            }
        }

        public bool IsTopic
        {
            get { return !IsSimpleQueue && string.IsNullOrEmpty(SubscriptionName); }
        }

        public override string ToString()
        {
            return _queueName;
        }

        public static QueueName FromSimpleQueueName(string name)
        {
            return new QueueName(name + "-" + name);
        }

        public static QueueName FromTopicName(string name)
        {
            return new QueueName(name + "-");
        }

        public static QueueName FromTopicAndSubscriptionName(string topicName, 
            string subscriptionName)
        {
            return new QueueName(topicName + "-" + subscriptionName);
        }

       
    }
}
