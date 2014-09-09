using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Visualiser.Model
{
    public class Topic
    {

        public Topic()
        {
            Subscriptions = new List<Subscription>();
        }

        public string Name { get; set; }

        public List<Subscription> Subscriptions { get; set; } 
    }
}
