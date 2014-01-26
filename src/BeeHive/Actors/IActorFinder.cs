using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Actors
{
    public interface IActorFinder
    {
        IEnumerable<IProcessorActor> Find(string queueName);
    }
}
