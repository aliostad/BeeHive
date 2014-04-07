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
            var id = Guid.NewGuid();
            store.InsertAsync(new TestCollectionEntity()
            {
                Id = id
            }).Wait();

            var t = store.GetAsync(id).Result;
            Assert.Equal(id, t.Id);
        }
    }

    public class TestCollectionEntity : IHaveIdentity
    {
        public Guid Id { get; set; }
    }
}
