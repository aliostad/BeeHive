#if NET461
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Configuration;
using Microsoft.Azure;


namespace BeeHive.Azure
{
    public class AzureConfigurationValueProvider : IConfigurationValueProvider
    {
        public string GetValue(string name)
        {
            return CloudConfigurationManager.GetSetting(name);
        }
    }
}
#else

using BeeHive.Configuration;
using Microsoft.Extensions.Configuration;

namespace BeeHive
{
    public class ConfigurationValueProvider : IConfigurationValueProvider
    {
        private IConfigurationProvider _configurationProvider;

        public ConfigurationValueProvider(IConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
        }

        public string GetValue(string name)
        {
            string value = null;
            _configurationProvider.TryGet(name, out value);
            return value;
        }
    }
}
#endif