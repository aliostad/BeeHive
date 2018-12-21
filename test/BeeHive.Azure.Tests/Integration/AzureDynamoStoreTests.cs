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
        public void ListItemsReturnsFolders()
        {
            var store = new AzureKeyValueStore(ConnectionString,ContainerName + Guid.NewGuid().ToString("N"));

            var root = Guid.NewGuid().ToString("N");
            var b1 = string.Format("{0}/a/b/{1}", root, Guid.NewGuid().ToString("N"));
            var b2 = string.Format("{0}/a/b/{1}", root, Guid.NewGuid().ToString("N"));
            var path1 = string.Format("{0}/a/", root);
            var path2 = string.Format("{0}/a/b/", root);
            

            store.InsertAsync(new SimpleBlob()
            {
                Body = new MemoryStream(),
                Id = b1
            }).Wait(); 
            store.InsertAsync(new SimpleBlob()
            {
                Body = new MemoryStream(),
                Id = b2
            }).Wait();

            var list = store.ListAsync(path1).Result.ToArray();
            var item = list.Single();
            Assert.Equal(path2, item.Id);

            var items = store.ListAsync(path2).Result.ToArray();
            Assert.Equal(b1, items[0].Id);
            Assert.Equal(b2, items[1].Id);

        }
    }
}
