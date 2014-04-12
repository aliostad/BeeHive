using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Configuration;
using Microsoft.WindowsAzure;

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
