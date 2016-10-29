using System;
using System.Diagnostics;
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
        private readonly string _cn;
        private readonly AzureLockStore _locker;

        public AzureLockStoreTests()
        {
            var s = Environment.GetEnvironmentVariable("abs_connection_string");
            _cn = string.IsNullOrEmpty(s) ? DefaultConnectionString : s;

            _locker = new AzureLockStore(
                new BlobSource()
                {
                    ContainerName = ContainerName,
                    ConnectionString = _cn,
                    Path = "this/is/great/"
                });
        }

        [Fact]
        public void TwoCannotLockAtTheSameTime()
        {
            var resource = Guid.NewGuid().ToString();
            var token = new LockToken(resource);

            var canLock = _locker.TryLockAsync(token).Result;
            Assert.True(canLock);

            var newtoken = new LockToken(resource);

            var canDoubleLock = _locker.TryLockAsync(newtoken,
                1, 100).Result;

            Assert.False(canDoubleLock);
        }

        [Fact]
        public void ICanLockForMoreThan30Seconds()
        {
            var locker = new AzureLockStore(
                new BlobSource()
                {
                    ContainerName = ContainerName,
                    ConnectionString = _cn,
                    Path = "this/is/great/"
                });

            var resource = Guid.NewGuid().ToString();
            var token = new LockToken(resource);

            var canLock = locker.TryLockAsync(token, 0, timeoutMilliseconds: 120*1000).Result;
            Assert.True(canLock);

            var newtoken = new LockToken(resource);
            Thread.Sleep(115 * 1000);
            var canDoubleLock = locker.TryLockAsync(newtoken,
                1, 100).Result;
            locker.ReleaseLockAsync(token).Wait();

            Assert.False(canDoubleLock);
        }

        [Fact]
        public void CanFailFastOnAquiringLock()
        {
            var resource = Guid.NewGuid().ToString();
            var token = new LockToken(resource);

            var locked = _locker.TryLockAsync(token).Result;
            Assert.True(locked);

            var newtoken = new LockToken(resource);
            var sw = Stopwatch.StartNew();
            var canDoubleLock = _locker.TryLockAsync(newtoken, 0, retryTimeoutMilliseconds: 100).Result;

            Assert.False(canDoubleLock);
            Assert.InRange(sw.Elapsed.TotalSeconds, 0, 2);
        }
    }
}
