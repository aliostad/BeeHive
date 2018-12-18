#if NET452
#else

using Microsoft.Extensions.Configuration;

namespace BeeHive.Configuration
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
