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
            var key = Guid.NewGuid();
            var listName = "myList";
            var inMemoryKeyedListStore = new InMemoryKeyedListStore<OrderWaitingForProduct>();
            inMemoryKeyedListStore.AddAsync(listName, key, new OrderWaitingForProduct()).Wait();
            Assert.True(inMemoryKeyedListStore.ListExistsAsync(listName, key).Result);
        }

        [Fact]
        public void TestAdd_ContainsItem()
        {
            var key = Guid.NewGuid();
            var listName = "myList";
            var itemId = Guid.NewGuid();
            var inMemoryKeyedListStore = new InMemoryKeyedListStore<OrderWaitingForProduct>();
            inMemoryKeyedListStore.AddAsync(listName, key, new OrderWaitingForProduct()
            {
                OrderId = itemId
            }).Wait();
            Assert.True(inMemoryKeyedListStore.ExistsAsync(listName, key, itemId).Result);
            
        }

        [Fact]
        public void TestAdd_DuplicateFails()
        {
            var key = Guid.NewGuid();
            var listName = "myList";
            var itemId = Guid.NewGuid();
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

    }
}
