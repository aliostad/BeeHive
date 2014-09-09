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
    public class AzureKeyValueStoreTests
    {

        private const string ConnectionString = "UseDevelopmentStorage=true;";
        private const string ContainerName = "band25";

        [Fact]
        public void ExistsReturnFalseForNonExistent()
        {
            var store = new AzureKeyValueStore(ConnectionString, ContainerName);
            var exists = store.ExistsAsync("adas/asdas/asdasdsa").Result;

            Assert.False(exists);
        }

        [Fact]
        public void CanInsertAndExistsReturnsTrue()
        {
            var store = new AzureKeyValueStore(ConnectionString, ContainerName);
            var simpleBlob = new SimpleBlob()
            {
                Body = new MemoryStream(new byte[4096]),
                Id = Guid.NewGuid().ToString("N")
            };
            store.InsertAsync(simpleBlob).Wait();
            Assert.True(store.ExistsAsync(simpleBlob.Id).Result);
        }

        [Fact]
        public void CanInsertAndGetBackMetadat()
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
            store.InsertAsync(simpleBlob).Wait();
            var metadata = store.GetMetadataAsync(simpleBlob.Id).Result;
            var blob2 = store.GetAsync(simpleBlob.Id).Result;

            var blockBlob = (CloudBlockBlob) blob2.UnderlyingBlob;
            Assert.Equal("b", metadata["a"]);
            Assert.Equal("d", metadata["c"]);
            Assert.Equal("image/png", blockBlob.Properties.ContentType);
        }
    }
}
