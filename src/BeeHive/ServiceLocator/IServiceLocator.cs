using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.ServiceLocator
{
    public interface IServiceLocator
    {
        object GetService(Type type);

        void ReleaseService(object service);
    }
}
