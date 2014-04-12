using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Configuration
{

    /// <summary>
    /// Generic configuration provider
    /// </summary>
    public interface IConfigurationValueProvider
    {
        string GetValue(string name);
    }
}
