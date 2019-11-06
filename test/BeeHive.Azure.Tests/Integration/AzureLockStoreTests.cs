using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using Microsoft.WindowsAzure.Storage;
using Xunit;

namespace BeeHive.Azure.Tests.Integration
{
    public class AzureLockStoreTests : BaseStorageTest
    {

        private const string ContainerName = "band25";
        private AzureLockStore _locker;

        private void OpenConnection()
        {
            _locker = new AzureLockStore(
                 new BlobSource()
                 {
                     ContainerName = ContainerName,
                     ConnectionString = ConnectionString,
                     Path = "this/is/great/"
                 });
        }

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task TwoCannotLockAtTheSameTime()
        {
            OpenConnection();
            var resource = Guid.NewGuid().ToString();
            var token = new LockToken(resource);

            var canLock = await  _locker.TryLockAsync(token);
            Assert.True(canLock);

            var newtoken = new LockToken(resource);

            var canDoubleLock = await _locker.TryLockAsync(newtoken,
                1, 100);

            Assert.False(canDoubleLock);
        }

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task ICanLockForMoreThan30Seconds()
        {
            var locker = new AzureLockStore(
                new BlobSource()
                {
                    ContainerName = ContainerName,
                    ConnectionString = ConnectionString,
                    Path = "this/is/great/"
                });

            var resource = Guid.NewGuid().ToString();
            var token = new LockToken(resource);

            var canLock = await  locker.TryLockAsync(token, 0, timeoutMilliseconds: 120*1000);
            Assert.True(canLock);

            var newtoken = new LockToken(resource);
            Thread.Sleep(115 * 1000);
            var canDoubleLock = await  locker.TryLockAsync(newtoken,
                1, 100);
            await locker.ReleaseLockAsync(token);

            Assert.False(canDoubleLock);
        }

        [EnvVarIgnoreFactAttribute(EnvVars.ConnectionStrings.AzureStorage)]
        public async Task CanFailFastOnAquiringLock()
        {
            OpenConnection();

            var resource = Guid.NewGuid().ToString();
            var token = new LockToken(resource);

            var locked = await _locker.TryLockAsync(token);
            Assert.True(locked);

            var newtoken = new LockToken(resource);
            var sw = Stopwatch.StartNew();
            var canDoubleLock = await _locker.TryLockAsync(newtoken, 0, aquireTimeoutMilliseconds: 100);

            Assert.False(canDoubleLock);
            Assert.InRange(sw.Elapsed.TotalSeconds, 0, 2);
        }
    }
}
