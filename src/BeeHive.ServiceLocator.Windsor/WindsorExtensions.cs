using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;

namespace BeeHive.ServiceLocator.Windsor
{
    public static class WindsorExtensions
    {
        public static BasedOnDescriptor ConfigureDependsOnNamed(this BasedOnDescriptor basedOnDescriptor)
        {
            return basedOnDescriptor.Configure(DependsOnNamedAction);
        }

        public static void DependsOnNamedAction(ComponentRegistration registration)
        {
            var implementationType = registration.Implementation;
            var constructor = implementationType.GetConstructors()
                .OrderByDescending(x=>x.GetParameters().Count()).FirstOrDefault();

            if(constructor==null)
                return;
            
            foreach (var parameterInfo in constructor.GetParameters())
            {
                var attribute = parameterInfo.GetCustomAttribute<DependsOnNamedAttribute>();
                if (attribute != null)
                    registration.DependsOn(Dependency.OnComponent(parameterInfo.ParameterType,
                        attribute.Name));
            }
        }


    }
}
