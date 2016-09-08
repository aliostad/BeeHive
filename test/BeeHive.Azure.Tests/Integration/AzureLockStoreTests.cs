using System;
using System.Threading;
using BeeHive.DataStructures;
using Microsoft.WindowsAzure.Storage;
using Xunit;

namespace BeeHive.Azure.Tests.Integration
{
    public class AzureLockStoreTests
    {

        private const string DefaultConnectionString = "UseDevelopmentStorage=true;";
        private const string ContainerName = "band25";
        private string _cn;

        public AzureLockStoreTests()
        {
            var s = Environment.GetEnvironmentVariable("abs_connection_string");
            _cn = string.IsNullOrEmpty(s) ? DefaultConnectionString : s;
        }


        [Fact]
        public void TwoCannotLockAtTheSameTime()
        {
            var locker = new AzureLockStore(
                new BlobSource()
                {
                    ContainerName = "band25",
                    ConnectionString = _cn,
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

        [Fact]
        public void ICanLockForMoreThan30Seconds()
        {
            var locker = new AzureLockStore(
                new BlobSource()
                {
                    ContainerName = "band25",
                    ConnectionString = _cn,
                    Path = "this/is/great/"
                });

            var resource = Guid.NewGuid().ToString();
            var token = new LockToken(resource);

            var canLock = locker.TryLockAsync(token, tries:0, timeoutMilliseconds:120*1000).Result;
            Assert.True(canLock);

            var newtoken = new LockToken(resource);
            Thread.Sleep(115 * 1000);
            var canDoubleLock = locker.TryLockAsync(newtoken,
                1, 100).Result;
            locker.ReleaseLockAsync(token).Wait();

            Assert.False(canDoubleLock);
        }
    }
}
