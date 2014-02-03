using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Actors;

namespace BeeHive
{
    public class Orchestrator : IService, IDisposable
    {
        private readonly IActorConfiguration _configuration;

        private List<IFactoryActor> _actors = new List<IFactoryActor>();
        private readonly IServiceLocator _serviceLocator;

        public Orchestrator(IActorConfiguration configuration, IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _configuration = configuration;
            foreach (var descriptor in _configuration.GetDescriptors())
            {
                var factoryActor = _serviceLocator.GetService<IFactoryActor>();
                factoryActor.Setup(descriptor);
                _actors.Add(factoryActor);
            }
        }


        public void Start()
        {
            _actors.ForEach(x => x.Start());
        }

        public void Stop()
        {
            _actors.ForEach(x => x.Stop());
        }

        public void Dispose()
        {
            foreach (var actor in _actors)
            {
                _serviceLocator.ReleaseService(actor);
            }
            _actors.Clear();
        }
    }
}
