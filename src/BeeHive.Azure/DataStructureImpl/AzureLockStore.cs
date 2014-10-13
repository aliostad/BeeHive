using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BeeHive.Azure
{
    public class AzureLockStore : ILockStore
    {
     
        private CloudBlobContainer _containerReference;
        private BlobSource _source;

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
            int timeoutMilliseconds = 15000)
        {
            if (tries < 1)
                tries = 1;

            var blob = await GetBlobAsync(token.ResourceId);
            for (int i = 0; i < tries; i++)
            {
                try
                {
                    await blob.AcquireLeaseAsync(
                        TimeSpan.FromMilliseconds(timeoutMilliseconds),
                        token.TokenId.ToString("N"));

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

        public async Task ReleaseLockAsync(LockToken token)
        {
            var blob = await GetBlobAsync(token.ResourceId);
            await blob.ReleaseLeaseAsync(AccessCondition.GenerateLeaseCondition(token.TokenId.ToString("N")));
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
                    TheTrace.TraceWarning(exception.ToString());
                }
            }

            return blob;
        }
    }
}
