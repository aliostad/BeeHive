using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive
{
    public class ActorDescriptor
    {
        public Type ActorType { get; set; }

        public string SourceQueueName { get; set; }

        public IInterval Interval { get; set; }

        public int DegreeOfParallelism { get; set; }

    }
}
