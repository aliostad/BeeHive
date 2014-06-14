using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Configuration
{
    public interface IStartup
    {
        void Start(IServiceLocator serviceLocator);
    }
}
