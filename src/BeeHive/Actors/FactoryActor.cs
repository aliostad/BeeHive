using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BeeHive.Scheduling;
using BeeHive.ServiceLocator;

namespace BeeHive.Actors
{
    public class FactoryActor : IFactoryActor
    {
        private readonly IServiceLocator _serviceLocator;
        private readonly ActorDescriptor _actorDescriptor;
        private readonly IQueueOperator _queueOperator;


        public FactoryActor(IServiceLocator serviceLocator, 
            ActorDescriptor actorDescriptor,
            IQueueOperator queueOperator)
        {
            _queueOperator = queueOperator;
            _actorDescriptor = actorDescriptor;
            _serviceLocator = serviceLocator;
            //new Poller(actorDescriptor.Interval, )
            
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
