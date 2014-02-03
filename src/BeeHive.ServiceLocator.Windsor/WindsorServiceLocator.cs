using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;

namespace BeeHive.ServiceLocator.Windsor
{
    public class WindsorServiceLocator : IServiceLocator
    {
        private readonly IWindsorContainer _container;

        public WindsorServiceLocator(IWindsorContainer container)
        {
            _container = container;
        }

        public object GetService(Type type)
        {
            return _container.Resolve(type);
        }
        
        public IEnumerable<object> GetServices(Type type)
        {
            return _container.ResolveAll(type)
                .Cast<object>();
        }
        public void ReleaseService(object service)
        {
            _container.Release(service);
        }

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}
