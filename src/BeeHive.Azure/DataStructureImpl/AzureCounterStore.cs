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

    

    /// <summary>
    /// Note: This is atomic but not performant as it uses locking. Currently not available on Azure
    /// </summary>
    public class AzureCounterStore : ICounterStore
    {
        private ILockStore _lockStore;
        private BlobSource _source;
        private CloudBlobContainer _containerReference;


        public AzureCounterStore(
            BlobSource source)
        {
            _source = source;
            _lockStore = new AzureLockStore(
                new BlobSource()
                {
                    ContainerName = source.ContainerName,
                    ConnectionString = source.ConnectionString,
                    Path = source.Path.TrimEnd('/') + "/" + "_locks_"
                });

            GetClientAndReference();
        }


        public async Task<long> GetAsync(string counterName, Guid id)
        {
            var key = string.Format("____COUNTER____{0}-{1}", counterName, id);
            var blob = await GetBlobAsync(key);
            var text = await blob.DownloadTextAsync();
            return long.Parse(text);
        }

        private void GetClientAndReference()
        {
            var account = CloudStorageAccount.Parse(_source.ConnectionString);
            var client = account.CreateCloudBlobClient();
            _containerReference = client.GetContainerReference(_source.ContainerName);
            _containerReference.CreateIfNotExists();
        }

        private async Task<CloudBlockBlob> GetBlobAsync(string key)
        {
            var blob = _containerReference.GetBlockBlobReference(_source.Path.TrimEnd('/') + "/" + key);
            if (!(await blob.ExistsAsync()))
            {
                try
                {
                    await blob.UploadFromStreamAsync(new MemoryStream());
                }
                catch (Exception exception) // someone else created it in the meanwhile
                {
                    Trace.TraceWarning(exception.ToString());
                }
            }

            return blob;
        }
        public async Task IncrementAsync(string counterName, Guid id, long value)
        {
            var key = string.Format("____COUNTER____{0}-{1}", counterName, id);
            var token = new LockToken(key);
            var result = await _lockStore.TryLockAsync(token);
            if(!result)
                throw new TimeoutException("Could not lock the resource");
            var blob = await GetBlobAsync(key);
            var text = await blob.DownloadTextAsync();
            var current = long.Parse(string.IsNullOrEmpty(text) ? "0" : text);
            var newValue = current + value;
            if(newValue < 0)
                throw new InvalidOperationException("Value cannot get below zero: " + newValue);
            
            await blob.UploadTextAsync(newValue.ToString());
            await _lockStore.ReleaseLockAsync(token);

        }
    }
}
