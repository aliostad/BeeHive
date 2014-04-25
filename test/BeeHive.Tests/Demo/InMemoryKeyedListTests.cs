using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.ConsoleDemo;
using BeeHive.DataStructures;
using BeeHive.Demo.PrismoEcommerce.WorkflowState;
using Xunit;

namespace BeeHive.Tests.Demo
{
    public class InMemoryKeyedListTests
    {
        [Fact]
        public void TestAdd_CreatesList()
        {
            var key = Guid.NewGuid().ToString("N");
            var listName = "myList";
            var inMemoryKeyedListStore = new InMemoryKeyedListStore<OrderWaitingForProduct>();
            inMemoryKeyedListStore.AddAsync(listName, key, new OrderWaitingForProduct()).Wait();
            Assert.True(inMemoryKeyedListStore.ListExistsAsync(listName, key).Result);
        }

        [Fact]
        public void TestAdd_ContainsItem()
        {
            var key = Guid.NewGuid().ToString("N");
            var listName = "myList";
            var itemId = Guid.NewGuid().ToString("N");
            var inMemoryKeyedListStore = new InMemoryKeyedListStore<OrderWaitingForProduct>();
            inMemoryKeyedListStore.AddAsync(listName, key, new OrderWaitingForProduct()
            {
                OrderId = itemId
            }).Wait();
            Assert.True(inMemoryKeyedListStore.ItemExistsAsync(listName, key, itemId).Result);
            
        }

        [Fact]
        public void TestAdd_DuplicateFails()
        {
            var key = Guid.NewGuid().ToString("N");
            var listName = "myList";
            var itemId = Guid.NewGuid().ToString("N");
            var inMemoryKeyedListStore = new InMemoryKeyedListStore<OrderWaitingForProduct>();
            inMemoryKeyedListStore.AddAsync(listName, key, new OrderWaitingForProduct()
            {
                OrderId = itemId
            }).Wait();
            Assert.Throws<AggregateException>(() => inMemoryKeyedListStore.AddAsync(listName, key, new OrderWaitingForProduct()
            {
                OrderId = itemId
            }).Wait());

        }

        [Fact]
        public void ConcurrencyCheck_Fails_WhenDifferentETags()
        {
            var key = Guid.NewGuid().ToString("N");
            var listName = "myList";
            var itemId = Guid.NewGuid().ToString("N");
            var inMemoryKeyedListStore = new InMemoryKeyedListStore<ParkedOrderItem>();
            var item = new ParkedOrderItem()
            {
                ProductId = itemId,
                ETag = "1234"
            };
            inMemoryKeyedListStore.AddAsync(listName, key, item).Wait();
            var item2 = new ParkedOrderItem()
            {
                ProductId = itemId,
                ETag = "5678"
            };
            Assert.Throws<AggregateException>(() => inMemoryKeyedListStore.UpdateAsync(listName, key, 
                item2).Wait());


        }

    }
}
