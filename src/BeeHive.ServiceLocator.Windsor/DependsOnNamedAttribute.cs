using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;

namespace BeeHive.ServiceLocator.Windsor
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class DependsOnNamedAttribute : Attribute
    {

        public DependsOnNamedAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public static void AddDependency(ComponentRegistration cr)
        {
            
        }
    
    }
   

}
