using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using Xunit;

namespace BeeHive.Azure.Tests.Integration
{
    public class AzureDynamoStoreTests : BaseStorageTest
    {
        private const string ContainerName = "band25";

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task ListItemsReturnsFolders()
        {
            var store = new AzureKeyValueStore(ConnectionString,ContainerName + Guid.NewGuid().ToString("N"));

            var root = Guid.NewGuid().ToString("N");
            var b1 = string.Format("{0}/a/b/{1}", root, Guid.NewGuid().ToString("N"));
            var b2 = string.Format("{0}/a/b/{1}", root, Guid.NewGuid().ToString("N"));
            var path1 = string.Format("{0}/a/", root);
            var path2 = string.Format("{0}/a/b/", root);
            

            await store.InsertAsync(new SimpleBlob()
            {
                Body = new MemoryStream(),
                Id = b1
            }); 
            
            await store.InsertAsync(new SimpleBlob()
            {
                Body = new MemoryStream(),
                Id = b2
            });

            var list = store.ListAsync(path1).Result.ToArray();
            var item = list.Single();
            Assert.Equal(path2, item.Id);

            var items = store.ListAsync(path2).Result.ToArray();
            Assert.Contains(b1, items.Select(x => x.Id));
            Assert.Contains(b2, items.Select(x => x.Id));

        }
    }
}
