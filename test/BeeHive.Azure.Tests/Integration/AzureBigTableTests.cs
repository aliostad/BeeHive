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

        [Fact]
        public void GetInsertAndGet()
        {
            var store = new AzureBigTableStore<HasIdentityAndRange>(connectionString:ConnectionString);
            var item = new HasIdentityAndRange()
            {
                Id = Guid.NewGuid().ToString(),
                RangeKey = "1",
                ETag = "\"" + Guid.NewGuid().ToString() + "\"",
                LastModified = DateTimeOffset.Now
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
                RangeKey = "1",
                LastModified = DateTimeOffset.Now
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
                RangeKey = "1",
                ETag = "\"" + Guid.NewGuid().ToString() + "\"",
                LastModified = DateTimeOffset.Now
            }; 
            var item2 = new HasIdentityAndRange()
            {
                Id = item.Id,
                RangeKey = "2",
                ETag = "\"" + Guid.NewGuid().ToString() + "\"",
                LastModified = DateTimeOffset.Now
            };
            store.InsertAsync(item).Wait();
            store.InsertAsync(item2).Wait();

            Assert.Equal(2, store.GetRangeAsync(item.Id, "1", "2").Result.Count());
        }



    }


    public class HasIdentityAndRange : IHaveIdentityAndRange, IConcurrencyAware
    {
        public string Id { get; set; }
        public string RangeKey { get; set; }
        public DateTimeOffset? LastModified { get; set; }
        public string ETag { get; set; }
    }

    public class FeedItem
    {

        public FeedItem()
        {
            PubDate = DateTimeOffset.Now;
        }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Link { get; set; }

        public string UniqueId { get; set; }

        public DateTimeOffset PubDate { get; set; }

        public string[] Categories { get; set; }

        public string ParentChannelUrl { get; set; }

        public string Author { get; set; }

        public string Id { get; set; }

        public string ChannelCategory { get; set; }

        public bool HasOriginalPubDate { get; set; }
    }

    public class FeedItemEntity : FeedItem, IHaveIdentityAndRange
    {
        public string RangeKey { get; set; }
    }
}
