using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using Microsoft.WindowsAzure.Storage.Blob;
using Xunit;

namespace BeeHive.Azure.Tests.Integration
{
    public class AzureKeyValueStoreTests : BaseStorageTest
    {

        private const string ContainerName = "band25";

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task ExistsReturnFalseForNonExistent()
        {
            var store = new AzureKeyValueStore(ConnectionString, ContainerName);
            var exists = await store.ExistsAsync("adas/asdas/asdasdsa");

            Assert.False(exists);
        }

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task CanInsertAndExistsReturnsTrue()
        {
            var store = new AzureKeyValueStore(ConnectionString, ContainerName);
            var simpleBlob = new SimpleBlob()
            {
                Body = new MemoryStream(new byte[4096]),
                Id = Guid.NewGuid().ToString("N")
            };
            await store.InsertAsync(simpleBlob);
            Assert.True(await store.ExistsAsync(simpleBlob.Id));
        }

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task CanInsertAndGetBackMetadat()
        {
            var store = new AzureKeyValueStore(ConnectionString, ContainerName);
            var simpleBlob = new SimpleBlob()
            {
                Body = new MemoryStream(new byte[4096]),
                Id = Guid.NewGuid().ToString("N"),
                Metadata = new Dictionary<string, string>()
                {
                    {"a", "b"},
                    {"c", "d"},
                    {"Content-Type", "image/png"}
                }
            };
            await store.InsertAsync(simpleBlob);
            var metadata = await store.GetMetadataAsync(simpleBlob.Id);
            var blob2 = await store.GetAsync(simpleBlob.Id);

            var blockBlob = (CloudBlockBlob) blob2.UnderlyingBlob;
            Assert.Equal("b", metadata["a"]);
            Assert.Equal("d", metadata["c"]);
            Assert.Equal("image/png", blockBlob.Properties.ContentType);
        }
    }
}
