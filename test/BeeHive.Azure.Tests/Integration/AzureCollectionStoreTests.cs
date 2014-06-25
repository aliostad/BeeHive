using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using Xunit;

namespace BeeHive.Azure.Tests.Integration
{
    public class AzureCollectionStoreTests
    {
        private const string ConnectionString = "UseDevelopmentStorage=true;";

        [Fact]
        public void CanStoreAndRetrieve()
        {
            var store = new AzureCollectionStore<TestCollectionEntity>(ConnectionString);
            var id = Guid.NewGuid().ToString("N");
            store.InsertAsync(new TestCollectionEntity()
            {
                Id = id
            }).Wait();

            Assert.True(store.ExistsAsync(id).Result);

            var t = store.GetAsync(id).Result;
            Assert.Equal(id, t.Id);
        }

        [Fact]
        public void CanStoreAndRemoved()
        {
            var store = new AzureCollectionStore<TestCollectionEntity>(ConnectionString);
            var id = Guid.NewGuid().ToString("N");
            var t = new TestCollectionEntity()
            {
                Id = id
            };
            store.InsertAsync(t).Wait();

            store.DeleteAsync(t).Wait();

            Assert.False(store.ExistsAsync(id).Result);
            Assert.Throws<AggregateException>(() => store.GetAsync(id).Result);
        }

        [Fact]
        public void CanBeUpdated()
        {
            var store = new AzureCollectionStore<TestCollectionEntity>(ConnectionString);
            var id = Guid.NewGuid().ToString("N");
            var t = new TestCollectionEntity()
            {
                Id = id,
                Name = "Tommy Shooter"
            };
            store.InsertAsync(t).Wait();
            t.Name = "The Fall";
            store.UpsertAsync(t).Wait();
            
            var t2 = store.GetAsync(id).Result;
            Assert.Equal("The Fall", t2.Name);
        }

        [Fact]
        public void CannotBeInsertedTwice()
        {
            var store = new AzureCollectionStore<TestCollectionEntity>(ConnectionString);
            var id = Guid.NewGuid().ToString("N");
            var t = new TestCollectionEntity()
            {
                Id = id,
                Name = "Tommy Shooter"
            };
            store.InsertAsync(t).Wait();
            Assert.Throws<AggregateException>(() => store.InsertAsync(t).Wait());
        }

        [Fact]
        public void CanBeUpdatedForConcurrencyAware()
        {
            var store = new AzureCollectionStore<TestCollectionEntityConcurrencyAware>(ConnectionString);
            var id = Guid.NewGuid().ToString("N");
            var t = new TestCollectionEntityConcurrencyAware()
            {
                Id = id,
                Name = "Tommy Shooter",
                ETag = "\"1234\"",
                LastModified = DateTimeOffset.UtcNow 
            };
            store.InsertAsync(t).Wait();
            t.Name = "The Fall";
            store.UpsertAsync(t).Wait();

            var t2 = store.GetAsync(id).Result;
            Assert.Equal("The Fall", t2.Name);
        }

        [Fact]
        public void ETagsProvidedWillBeUsedAndStored()
        {
            var store = new AzureCollectionStore<TestCollectionEntityConcurrencyAware>(ConnectionString);
            var id = Guid.NewGuid().ToString("N");
            var t = new TestCollectionEntityConcurrencyAware()
            {
                Id = id,
                Name = "Tommy Shooter",
                ETag = "\"1234\"",
                LastModified = DateTimeOffset.UtcNow.AddHours(1)
            };
            store.InsertAsync(t).Wait();
            var entity = store.GetAsync(id).Result;
            Assert.Equal(t.ETag, entity.ETag);
        }

        [Fact]
        public void CannotBeUpdatedForConcurrencyAwareWhenETagInConflict()
        {
            var store = new AzureCollectionStore<TestCollectionEntityConcurrencyAware>(ConnectionString);
            var id = Guid.NewGuid().ToString("N");
            var t = new TestCollectionEntityConcurrencyAware()
            {
                Id = id,
                Name = "Tommy Shooter",
                ETag = "\"1234\"",
                LastModified = DateTimeOffset.UtcNow
                
            };
            store.InsertAsync(t).Wait();

            t.Name = "The Fall";
            t.ETag = "\"8998\"";
            
            Assert.Throws<AggregateException>(() => store.UpsertAsync(t).Wait());
        }

        [Fact]
        public void CannotBeDeletedForConcurrencyAwareWhenETagInConflict()
        {
            var store = new AzureCollectionStore<TestCollectionEntityConcurrencyAware>(ConnectionString);
            var id = Guid.NewGuid().ToString("N");
            var t = new TestCollectionEntityConcurrencyAware()
            {
                Id = id,
                Name = "Tommy Shooter",
                ETag = "1234",
                LastModified = DateTimeOffset.UtcNow
            };
            store.InsertAsync(t).Wait();

            t.Name = "The Fall";
            t.ETag = "4567";

            Assert.Throws<AggregateException>(() => store.DeleteAsync(t).Wait());
        }


    }

    public class TestCollectionEntity : IHaveIdentity
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class TestCollectionEntityConcurrencyAware : IHaveIdentity, IConcurrencyAware
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset? LastModified { get; set; }
        public string ETag { get; set; }
    }

}
