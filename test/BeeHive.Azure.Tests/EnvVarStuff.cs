using System;
using Xunit;

namespace BeeHive.Azure.Tests
{
    class EnvVarIgnoreFactAttribute : FactAttribute
    {
        public EnvVarIgnoreFactAttribute(string envVar)
        {
            var env = Environment.GetEnvironmentVariable(envVar);
            if (string.IsNullOrEmpty(env))
            {
                Skip = $"Please set {envVar} env var to run.";
            }
        }
    }

    class EnvVarIgnoreTheoryAttribute : TheoryAttribute
    {
        public EnvVarIgnoreTheoryAttribute(string envVar)
        {
            var env = Environment.GetEnvironmentVariable(envVar);
            if (string.IsNullOrEmpty(env))
            {
                Skip = $"Please set {envVar} env var to run.";
            }
        }
    }

    static class EnvVars
    {
        public static class ConnectionStrings
        {
            public const string ServiceBus = "azure_service_bus_connection_string";
            public const string AzureStorage = "azure_storage_connection_string";
        }
    }

}
