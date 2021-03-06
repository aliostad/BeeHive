﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using Xunit;

namespace BeeHive.Azure.Tests.Integration
{
    public class AzureCollectionStoreTests : BaseStorageTest
    {
        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task CanStoreAndRetrieve()
        {
            var store = new AzureCollectionStore<TestCollectionEntity>(ConnectionString);
            var id = Guid.NewGuid().ToString("N");
            await store.InsertAsync(new TestCollectionEntity()
            {
                Id = id
            });

            Assert.True(await store.ExistsAsync(id));

            var t = await store.GetAsync(id);
            Assert.Equal(id, t.Id);
        }

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task CanStoreAndRemoved()
        {
            var store = new AzureCollectionStore<TestCollectionEntity>(ConnectionString);
            var id = Guid.NewGuid().ToString("N");
            var t = new TestCollectionEntity()
            {
                Id = id
            };
            await store.InsertAsync(t);

            await store.DeleteAsync(t);

            Assert.False(await store.ExistsAsync(id));
            await Assert.ThrowsAsync<AggregateException>(() => store.GetAsync(id));
        }

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task CanBeUpdated()
        {
            var store = new AzureCollectionStore<TestCollectionEntity>(ConnectionString);
            var id = Guid.NewGuid().ToString("N");
            var t = new TestCollectionEntity()
            {
                Id = id,
                Name = "Tommy Shooter"
            };
            await store.InsertAsync(t);
            t.Name = "The Fall";
            await store.UpsertAsync(t);
            
            var t2 = await store.GetAsync(id);
            Assert.Equal("The Fall", t2.Name);
        }

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task CannotBeInsertedTwice()
        {
            var store = new AzureCollectionStore<TestCollectionEntity>(ConnectionString);
            var id = Guid.NewGuid().ToString("N");
            var t = new TestCollectionEntity()
            {
                Id = id,
                Name = "Tommy Shooter"
            };
            await store.InsertAsync(t);
            await Assert.ThrowsAsync<AggregateException>(() => store.InsertAsync(t));
        }

        [Fact(Skip = "Azure have broken it on latest releases")]
        public async Task CanBeUpdatedForConcurrencyAware()
        {
            var store = new AzureCollectionStore<TestCollectionEntityConcurrencyAware>(ConnectionString);
            var id = Guid.NewGuid().ToString("N");
            var t = new TestCollectionEntityConcurrencyAware()
            {
                Id = id,
                Name = "Tommy Shooter",
                ETag = "W/\"1234798h88787\"",
                LastModified = DateTimeOffset.UtcNow 
            };
            await store.InsertAsync(t);
            t.Name = "The Fall";
            var tr = await store.GetAsync(id);
            await store.UpsertAsync(tr);

            var t2 = await store.GetAsync(id);
            Assert.Equal("The Fall", t2.Name);
        }

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task ETagsProvidedWillBeUsedAndStored()
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
            await store.InsertAsync(t);
            var entity = await store.GetAsync(id);
            Assert.Equal(t.ETag, entity.ETag);
        }

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task CannotBeUpdatedForConcurrencyAwareWhenETagInConflict()
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
            
            await store.InsertAsync(t);

            t.Name = "The Fall";
            t.ETag = "\"8998\"";
            
            await Assert.ThrowsAsync<AggregateException>(() => store.UpsertAsync(t));
        }

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task CannotBeDeletedForConcurrencyAwareWhenETagInConflict()
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
            await store.InsertAsync(t);

            t.Name = "The Fall";
            t.ETag = "4567";

            await Assert.ThrowsAsync<AggregateException>(() => store.DeleteAsync(t));
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
