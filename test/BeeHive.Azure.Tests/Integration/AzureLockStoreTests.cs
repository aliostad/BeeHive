using System;
using BeeHive.DataStructures;
using Microsoft.WindowsAzure.Storage;
using Xunit;

namespace BeeHive.Azure.Tests.Integration
{
    public class AzureLockStoreTests
    {

        private const string ConnectionString = "UseDevelopmentStorage=true;";
        private const string ContainerName = "band25";

       

        [Fact]
        public void TwoCannotLockAtTheSameTime()
        {
            var locker = new AzureLockStore(
                new BlobSource()
                {
                    ContainerName = "band25",
                    ConnectionString = ConnectionString,
                    Path = "this/is/great/"
                });

            var resource = Guid.NewGuid().ToString();
            var token = new LockToken(resource);

            var canLock = locker.TryLockAsync(token).Result;
            Assert.True(canLock);

            var newtoken = new LockToken(resource);

            var canDoubleLock = locker.TryLockAsync(newtoken,
                1, 100).Result;

            Assert.False(canDoubleLock);
        }
    }
}
