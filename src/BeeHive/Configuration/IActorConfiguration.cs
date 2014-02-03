using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Actors;

namespace BeeHive
{
    public interface IActorConfiguration
    {
        IEnumerable<ActorDescriptor> GetDescriptors();
    }
}
