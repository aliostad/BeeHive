using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using Xunit;

namespace BeeHive.Azure.Tests.Integration
{
    public class AzureBigTableTests
    {
        private const string ConnectionString = "UseDevelopmentStorage=true;";

        private const string ContainerName = "band25";

        [Fact]
        public void GetInsertAndGet()
        {
            var store = new AzureBigTableStore<HasIdentityAndRange>(connectionString:ConnectionString);
            var item = new HasIdentityAndRange()
            {
                Id = Guid.NewGuid().ToString(),
                RangeKey = "1"
            };
            store.InsertAsync(item).Wait();

            Assert.True(store.ExistsAsync(item.Id,item.RangeKey).Result);
        }

        [Fact]
        public void CanInsertAndDelete()
        {
            var store = new AzureBigTableStore<HasIdentityAndRange>(connectionString: ConnectionString);
            var item = new HasIdentityAndRange()
            {
                Id = Guid.NewGuid().ToString(),
                RangeKey = "1"
            };
            store.InsertAsync(item).Wait();
            store.DeleteAsync(item).Wait();

            Assert.False(store.ExistsAsync(item.Id, item.RangeKey).Result);
        }


        [Fact]
        public void CanGetRange()
        {
            var store = new AzureBigTableStore<HasIdentityAndRange>(connectionString: ConnectionString);
            var item = new HasIdentityAndRange()
            {
                Id = Guid.NewGuid().ToString(),
                RangeKey = "1"
            }; 
            var item2 = new HasIdentityAndRange()
            {
                Id = item.Id,
                RangeKey = "2"
            };
            store.InsertAsync(item).Wait();
            store.InsertAsync(item2).Wait();

            Assert.Equal(2, store.GetRangeAsync(item.Id, "1", "2").Result.Count());
        }



    }


    public class HasIdentityAndRange : IHaveIdentityAndRange
    {
        public string Id { get; set; }
        public string RangeKey { get; set; }
    }

}
