using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BeeHive.Azure.Tests.Integration
{

    public class AzureCounterStoreTests: BaseStorageTest
    {
        private const string ContainerName = "band25";

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task ConcurrentIncrementWorks()
        {
            ThreadPool.SetMinThreads(20, 20);
            var key = Guid.NewGuid().ToString("N");
            const string CounterName = "TestCounter";
            const int Increment = 10;
            const int ParallelClients = 5;

            var counter = new AzureCounterStore(new BlobSource()
            {
                ConnectionString = ConnectionString,
                ContainerName = ContainerName,
                Path = string.Format("I/Have/Got/A/Dream/{0}/", Guid.NewGuid().ToString("N")) 
            });

            Parallel.For(0, ParallelClients, (i) =>
            {
                counter.IncrementAsync(CounterName, key, Increment).ConfigureAwait(false).GetAwaiter().GetResult();
            });

            var result = await counter.GetAsync(CounterName, key);
            Assert.Equal(ParallelClients * Increment, result);
        }
    }
}
