using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BeeHive.Azure
{
    public class AzureKeyValueStore : IDynamoStore
    {
        private CloudBlobContainer _container;

        public AzureKeyValueStore(string connectionString,
            string bucketName)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudBlobClient();
            _container = client.GetContainerReference(bucketName);
            _container.CreateIfNotExists();
        }

        public async Task<IBlob> GetAsync(string id)
        {
            var blobReference = _container.GetBlockBlobReference(id);
            await blobReference.FetchAttributesAsync();
            var blob = new SimpleBlob()
            {
                Body = blobReference.ToStream(),
                Metadata = new Dictionary<string, string>(blobReference.Metadata),
                ETag = blobReference.Properties.ETag,
                LastModified = blobReference.Properties.LastModified,
                Id = id,
                UnderlyingBlob = blobReference
            };

            return blob;

        }

        public async Task<IEnumerable<IBlob>> ListAsync(string path, bool flatSearch = false)
        {
            return _container.ListBlobs(path, flatSearch)
                .Select(x => x.ToBlob());
        }

        public async Task<Dictionary<string, string>> GetMetadataAsync(string id)
        {
            var blobReference = _container.GetBlockBlobReference(id);
            await blobReference.FetchAttributesAsync();
            return new Dictionary<string, string>(blobReference.Metadata);
        }

        public async Task InsertAsync(IBlob t)
        {
            var blobReference = _container.GetBlockBlobReference(t.Id);
            if(await blobReference.ExistsAsync())
                throw new InvalidOperationException("Key already exists: " + t.Id);

            await UpsertAsync(t);
        }

        public async Task UpsertAsync(IBlob t)
        {

            var blobReference = _container.GetBlockBlobReference(t.Id);

            // concurrency check
            var concurrencyAware = t as IConcurrencyAware;
            CheckConcurrency(concurrencyAware, blobReference);


            if (t.Metadata != null)
            {
                foreach (var kv in t.Metadata)
                {
                    if (kv.Key.Equals("content-type", StringComparison.InvariantCultureIgnoreCase))
                    {
                        blobReference.Properties.ContentType = kv.Value;
                        continue;
                    }

                    // TODO: do the rest of properties


                    blobReference.Metadata[kv.Key] = kv.Value;
                }

            }
            await blobReference.UploadFromStreamAsync(t.Body);
            if (t.Metadata != null)
            {
                await blobReference.SetMetadataAsync();
            }

            t.UnderlyingBlob = blobReference;

        }

        private static void CheckConcurrency(IConcurrencyAware concurrencyAware,
            CloudBlockBlob blobReference)
        {
            if (concurrencyAware != null)
            {
                if (!string.IsNullOrEmpty(concurrencyAware.ETag) &&
                    blobReference.Properties.ETag != concurrencyAware.ETag)
                {
                    throw new ConcurrencyConflictException(concurrencyAware.ETag,
                        blobReference.Properties.ETag);
                }

                if (concurrencyAware.LastModified != null &&
                    blobReference.Properties.LastModified != null &&
                   blobReference.Properties.LastModified != concurrencyAware.LastModified)
                {
                    throw new ConcurrencyConflictException(concurrencyAware.LastModified.Value,
                        blobReference.Properties.LastModified.Value);
                }
            }

        }

        public async Task DeleteAsync(IBlob t)
        {
            var blobReference = _container.GetBlockBlobReference(t.Id);
            // concurrency check
            var concurrencyAware = t as IConcurrencyAware;
            CheckConcurrency(concurrencyAware, blobReference);

            await blobReference.DeleteAsync();

        }

        public async Task<bool> ExistsAsync(string id)
        {
            var blobReference = _container.GetBlockBlobReference(id);
            return await blobReference.ExistsAsync();
        }

     
    }
}
