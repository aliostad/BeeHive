using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity;

namespace BeeHive.ServiceLocator.Unity
{
    public class UnityServiceLocator : IServiceLocator
    {
        private readonly IUnityContainer container;

        public UnityServiceLocator(IUnityContainer container)
        {
            this.container = container;
        }

        public object GetService(Type type)
        {
            return container.Resolve(type);
        }

        public IEnumerable<object> GetServices(Type type)
        {
            return container.ResolveAll(type);
        }

        public void ReleaseService(object service)
        {
            var disposable = service as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        public void Dispose()
        {
            container.Dispose();
        }
    }
}
