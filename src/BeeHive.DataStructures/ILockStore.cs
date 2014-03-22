using System;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{
    public interface ILockStore
    {
        Task<bool> TryLockAsync(LockToken token, int tries = 16, 
            int retryTimeoutMilliseconds = 15000, 
            int timeoutMilliseconds = 15000);

        Task ReleaseLockAsync(LockToken token);

    }
}
