using Amazon;
using BeeHive.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BeeHive.Aws.Tests.Integration
{
    public class AwsKeyValueStoreTests
    {
        private const string BucketName = "band25"; // THIS BUCKET MUST EXIST
        private const string Id = "sumo/shabalo/oh/shabalow";

        [EnvVarIgnoreFactAttribute(EnvVars.Key)]
        public async Task ExistsReturnFalseForNonExistent()
        {
            var store = new AwsKeyValueStore(RegionEndpoint.EUWest1, BucketName);
            var exists = await store.ExistsAsync("adas/asdas/asdasdsa");

            Assert.False(exists);
        }

        [EnvVarIgnoreFactAttribute(EnvVars.Key)]
        public async Task CanInsert()
        {
            var store = new AwsKeyValueStore(RegionEndpoint.EUWest1, BucketName);
            await store.InsertAsync(
                new SimpleBlob()
                {
                    Body = new MemoryStream(new byte[256]),
                    Id = Id
                });

            Assert.True(await store.ExistsAsync(Id));
        }

        [EnvVarIgnoreFactAttribute(EnvVars.Key)]
        public async Task CanUpsert()
        {
            var store = new AwsKeyValueStore(RegionEndpoint.EUWest1, BucketName);
            await store.UpsertAsync(
                new SimpleBlob()
                {
                    Body = new MemoryStream(new byte[256]),
                    Id = Id
                });

            Assert.True(await store.ExistsAsync(Id));
        }

        [EnvVarIgnoreFactAttribute(EnvVars.Key)]
        public async Task CanDelete()
        {
            var store = new AwsKeyValueStore(RegionEndpoint.EUWest1, BucketName);
            await store.UpsertAsync(
                new SimpleBlob()
                {
                    Body = new MemoryStream(new byte[256]),
                    Id = Id
                });

            await store.DeleteAsync(
                new SimpleBlob()
                {
                    Id = Id
                });

            Assert.False(await store.ExistsAsync(Id));
        }

    }
}
