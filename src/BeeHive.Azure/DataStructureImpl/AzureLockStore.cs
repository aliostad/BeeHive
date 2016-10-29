using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
#pragma warning disable 4014

namespace BeeHive.Azure
{
    public class AzureLockStore : ILockStore
    {
     
        private CloudBlobContainer _containerReference;
        private BlobSource _source;

        const int MaxLockPossibleMilliseconds = 30*1000;

        public AzureLockStore(BlobSource source)
        {
            _source = source;
            GetClientAndReference();
        }

        private void GetClientAndReference()
        {    
            var account = CloudStorageAccount.Parse(_source.ConnectionString);
            var client = account.CreateCloudBlobClient();
            _containerReference = client.GetContainerReference(_source.ContainerName);
            _containerReference.CreateIfNotExists();
        }


        public async Task<bool> TryLockAsync(
            LockToken token, 
            int tries = 16, // this is NOT retry - it is try
            int retryTimeoutMilliseconds = 15000, 
            int timeoutMilliseconds = 15000,
            int aquireTimeoutMilliseconds = 15000)
        {
            if (tries < 1)
                tries = 1;

            var blob = await GetBlobAsync(token.ResourceId);
            for (var i = 0; i < tries; i++)
            {
                try
                {
                    using (var cancellationTokenSource = new CancellationTokenSource(aquireTimeoutMilliseconds))
                    {
                        await blob.AcquireLeaseAsync(
                            TimeSpan.FromMilliseconds(Math.Min(MaxLockPossibleMilliseconds, timeoutMilliseconds)),
                            token.TokenId.ToString("N"), cancellationTokenSource.Token);
                    }

                    if (timeoutMilliseconds > MaxLockPossibleMilliseconds)
                    {
                        KeepExtendingLeaseAsync(async () =>
                        {
                            await blob.AcquireLeaseAsync(
                                TimeSpan.FromMilliseconds(MaxLockPossibleMilliseconds),
                                token.TokenId.ToString("N"));
                        }, TimeSpan.FromMilliseconds( MaxLockPossibleMilliseconds), token.RenewCancellation.Token, token.ResourceId); // DO NOT WAIT THIS!!!                       
                    }
                    
                    return true;
                }
                catch (Exception e)
                {
                    TheTrace.TraceInformation("Lock attempt - already locked: {0}", e);
                    // ignore
                }
                await Task.Delay(TimeSpan.FromMilliseconds(retryTimeoutMilliseconds / tries));
            }

            return false;
        }

        private async Task KeepExtendingLeaseAsync(Func<Task> extendLeaseAsync, TimeSpan howLong, CancellationToken cancellationToken, string resource)
        {

            var thisLong = new TimeSpan(2*howLong.Ticks/3); // RATM: how long? This long, what you reap is what you sew!
            await Task.Delay(thisLong, cancellationToken);

            while (true)
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    await extendLeaseAsync();
                    TheTrace.TraceInformation("Extended the lifetime of the lease for {0}...", resource);
                    await Task.Delay(thisLong, cancellationToken);
                }
                catch (Exception exception)
                {
                    TheTrace.TraceError(exception.ToString());
                    break;
                }
            }
        }

        public async Task ReleaseLockAsync(LockToken token)
        {
            var blob = await GetBlobAsync(token.ResourceId);
            await blob.ReleaseLeaseAsync(AccessCondition.GenerateLeaseCondition(token.TokenId.ToString("N")));
            token.RenewCancellation.Cancel();
        }

        private async Task<CloudBlockBlob> GetBlobAsync(string key)
        {
             var blob = _containerReference.GetBlockBlobReference(_source.Path.TrimEnd('/') + "/" + key);
            if (! (await blob.ExistsAsync()))
            {
                try
                {
                    await blob.UploadFromStreamAsync(new MemoryStream());
                }
                catch (Exception exception) // someone else created it in the meanwhile
                {
                    TheTrace.TraceWarning("someone else created {0} meanwhile: {1}", key, exception.ToString());
                }
            }

            return blob;
        }
    }
}
