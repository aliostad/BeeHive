using System;
using System.Collections.Generic;
using System.Text;

namespace BeeHive.Azure.Tests.Integration
{
    public class BaseStorageTest
    {
        protected string ConnectionString = null;

        public BaseStorageTest()
        {
            ConnectionString = Environment.GetEnvironmentVariable(EnvVars.ConnectionStrings.AzureStorage);
        }
    }
}
