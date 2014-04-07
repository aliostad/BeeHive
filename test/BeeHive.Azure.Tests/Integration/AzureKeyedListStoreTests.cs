using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using Xunit;

namespace BeeHive.Azure.Tests.Integration
{
    public class AzureKeyedListStoreTests
    {
        private const string ConnectionString = "UseDevelopmentStorage=true;";
        private const string ListName = "test";
        


        [Fact]
        public void Stores()
        {
            var store = new AzureKeyedListStore<TestEntity>(ConnectionString);
            var entity = new TestEntity(){Name = "chipabla"};
            var against = Guid.NewGuid();
            store.AddAsync(ListName, against , entity).Wait();

            var values = store.GetAsync(ListName, against).Result;
            var firstOrDefault = values.FirstOrDefault(x=> x.Id == entity.Id);
            Assert.NotNull(firstOrDefault);
            Assert.Equal(entity.Name, firstOrDefault.Name);
            Assert.Equal(entity.Id, firstOrDefault.Id);
        }
    

        [Fact]
        public void Updates()
        {
            var store = new AzureKeyedListStore<TestEntity>(ConnectionString);
            var entity = new TestEntity(){Name = "chipabla"};
            var against = Guid.NewGuid();
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

        [Fact]
        public void Removes()
        {
            var store = new AzureKeyedListStore<TestEntity>(ConnectionString);
            var entity = new TestEntity() { Name = "chipabla" };
            var against = Guid.NewGuid();
            store.AddAsync(ListName, against, entity).Wait();
            entity.Name = "chobandikola";

            // act
            store.RemoveAsync(ListName, against).Wait();

            // assert
            
            Assert.False(store.ExistsAsync(ListName, against).Result);
        }

        [Fact]
        public void ListExists_ForExisting()
        {
            var newListName = "Table" + Guid.NewGuid().ToString("N").Substring(10);
            var store = new AzureKeyedListStore<TestEntity>(ConnectionString);
            var entity = new TestEntity() { Name = "chipabla" };
            var against = Guid.NewGuid();
            store.AddAsync(newListName, against, entity).Wait();

            Assert.True(store.ListExistsAsync(newListName).Result);
        }

        [Fact]
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
            Id = Guid.NewGuid();
        }

        public string Name { get; set; }

        public Guid Id { get; set; }
    }
}
