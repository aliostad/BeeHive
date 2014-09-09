using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Visualiser.Model
{
    public class Subscription
    {
        public QueueName Name { get; set; }

        public Topic Topic { get; set; }

        public ReactiveActor Actor { get; set; }
    }
}
