using System;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{
    public interface ILockStore
    {
        Task<bool> TryLockAsync(LockToken token, int tries = 16, 
            int retryTimeoutMilliseconds = 3000, 
            int timeoutMilliseconds = 10000);

        Task ReleaseLockAsync(LockToken token);

    }
}
