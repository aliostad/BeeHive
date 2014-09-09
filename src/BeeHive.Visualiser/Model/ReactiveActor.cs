using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Visualiser.Model
{
    public class ReactiveActor
    {

        public ReactiveActor()
        {
            SendsTo = new List<Topic>();
        }

        public List<Topic> SendsTo { get; set; }

        public Subscription ReceivesFrom { get; set; }

        public string Name { get; set; }
    }
}
