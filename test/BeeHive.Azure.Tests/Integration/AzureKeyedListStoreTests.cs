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
        public void Stores()
        {
            var store = new AzureKeyedListStore<TestEntity>(ConnectionString);
            var entity = new TestEntity(){Name = "chipabla"};
            var against = Guid.NewGuid().ToString("N");
            store.AddAsync(ListName, against , entity).Wait();

            var values = store.GetAsync(ListName, against).Result;
            var firstOrDefault = values.FirstOrDefault(x=> x.Id == entity.Id);
            Assert.NotNull(firstOrDefault);
            Assert.Equal(entity.Name, firstOrDefault.Name);
            Assert.Equal(entity.Id, firstOrDefault.Id);
        }


        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public void Updates()
        {
            var store = new AzureKeyedListStore<TestEntity>(ConnectionString);
            var entity = new TestEntity(){Name = "chipabla"};
            var against = Guid.NewGuid().ToString("N");
            store.AddAsync(ListName, against , entity).Wait();
            entity.Name = "chobandikola";

            // act
            store.UpdateAsync(ListName, against, entity).Wait();
            
            // assert
            var values = store.GetAsync(ListName, against).Result;
            var firstOrDefault = values.FirstOrDefault(x=> x.Id == entity.Id);
            Assert.NotNull(firstOrDefault);
            Assert.Equal(entity.Name, firstOrDefault.Name);
            Assert.Equal(entity.Id, firstOrDefault.Id);
        }

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public void Removes()
        {
            var store = new AzureKeyedListStore<TestEntity>(ConnectionString);
            var entity = new TestEntity() { Name = "chipabla" };
            var against = Guid.NewGuid().ToString("N");
            store.AddAsync(ListName, against, entity).Wait();
            entity.Name = "chobandikola";

            // act
            store.RemoveAsync(ListName, against).Wait();

            // assert
            Assert.False(store.ExistsAsync(ListName, against).Result);
        }

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public void ListExists_ForExisting()
        {
            var newListName = "Table" + Guid.NewGuid().ToString("N").Substring(10);
            var store = new AzureKeyedListStore<TestEntity>(ConnectionString);
            var entity = new TestEntity() { Name = "chipabla" };
            var against = Guid.NewGuid().ToString("N");
            store.AddAsync(newListName, against, entity).Wait();

            Assert.True(store.ListExistsAsync(newListName).Result);
        }

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public void ListDoesntExists_ForNonExisting()
        {
            var newListName = "Table" + Guid.NewGuid().ToString("N").Substring(10);
            var store = new AzureKeyedListStore<TestEntity>(ConnectionString);
            Assert.False(store.ListExistsAsync(newListName).Result);
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
