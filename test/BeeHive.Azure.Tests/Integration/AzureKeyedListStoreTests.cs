using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using Xunit;

namespace BeeHive.Azure.Tests.Integration
{
    public class AzureKeyedListStoreTests : BaseStorageTest
    {
        private const string ListName = "test";

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task Stores()
        {
            var store = new AzureKeyedListStore<TestEntity>(ConnectionString);
            var entity = new TestEntity(){Name = "chipabla"};
            var against = Guid.NewGuid().ToString("N");
            await store.AddAsync(ListName, against , entity);

            var values = await store.GetAsync(ListName, against);
            var firstOrDefault = values.FirstOrDefault(x=> x.Id == entity.Id);
            Assert.NotNull(firstOrDefault);
            Assert.Equal(entity.Name, firstOrDefault.Name);
            Assert.Equal(entity.Id, firstOrDefault.Id);
        }


        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task Updates()
        {
            var store = new AzureKeyedListStore<TestEntity>(ConnectionString);
            var entity = new TestEntity(){Name = "chipabla"};
            var against = Guid.NewGuid().ToString("N");
            await store.AddAsync(ListName, against , entity);
            entity.Name = "chobandikola";

            // act
            await store.UpdateAsync(ListName, against, entity);
            
            // assert
            var values = await  store.GetAsync(ListName, against);
            var firstOrDefault = values.FirstOrDefault(x=> x.Id == entity.Id);
            Assert.NotNull(firstOrDefault);
            Assert.Equal(entity.Name, firstOrDefault.Name);
            Assert.Equal(entity.Id, firstOrDefault.Id);
        }

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task Removes()
        {
            var store = new AzureKeyedListStore<TestEntity>(ConnectionString);
            var entity = new TestEntity() { Name = "chipabla" };
            var against = Guid.NewGuid().ToString("N");
            await store.AddAsync(ListName, against, entity);
            entity.Name = "chobandikola";

            // act
            await store.RemoveAsync(ListName, against);

            // assert
            Assert.False(await store.ExistsAsync(ListName, against));
        }

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task ListExists_ForExisting()
        {
            var newListName = "Table" + Guid.NewGuid().ToString("N").Substring(10);
            var store = new AzureKeyedListStore<TestEntity>(ConnectionString);
            var entity = new TestEntity() { Name = "chipabla" };
            var against = Guid.NewGuid().ToString("N");
            await store.AddAsync(newListName, against, entity);

            Assert.True(await store.ListExistsAsync(newListName));
        }

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task ListDoesntExists_ForNonExisting()
        {
            var newListName = "Table" + Guid.NewGuid().ToString("N").Substring(10);
            var store = new AzureKeyedListStore<TestEntity>(ConnectionString);
            Assert.False(await store.ListExistsAsync(newListName));
        }

    }

    public class TestEntity : IHaveIdentity
    {
        public TestEntity()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        public string Name { get; set; }

        public string Id { get; set; }
    }
}
