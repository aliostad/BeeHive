using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using BeeHive.DataStructures;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Threading.Tasks;

namespace BeeHive.Aws
{
    public class AwsKeyValueStore : IKeyValueStore
    {
        private readonly IAmazonS3 _client;
        private readonly string _bucketName;

        public AwsKeyValueStore(RegionEndpoint region, string bucketName)
        {
            _client = new AmazonS3Client(region);
            _bucketName = bucketName;
        }

        public Task DeleteAsync(IBlob t)
        {
            return _client.DeleteObjectAsync(_bucketName, t.Id);
        }

        public Task<bool> ExistsAsync(string id)
        {
            return ExistsAsync(_client, _bucketName, id);
        }

        internal static async Task<bool> ExistsAsync(IAmazonS3 client, string bucketName, string id)
        {
            try
            {
                var res = await client.GetObjectMetadataAsync(new GetObjectMetadataRequest()
                {
                    BucketName = bucketName,
                    Key = id
                });

                return true;
            }
            catch (AmazonS3Exception s3ex)
            {
                if (s3ex.StatusCode == HttpStatusCode.NotFound)
                    return false;
                else
                    throw;
            }
        }

        public async Task<IBlob> GetAsync(string id)
        {

            var res = await _client.GetObjectMetadataAsync(new GetObjectMetadataRequest()
            {
                BucketName = _bucketName,
                Key = id
            });

            var dic = new Dictionary<string, string>();
            foreach(var k in res.Metadata.Keys)
            {
                dic[k] = res.Metadata[k];
            }

            return new SimpleBlob()
            {
                Body = await _client.GetObjectStreamAsync(_bucketName, id, null),
                ETag = res.ETag,
                IsVirtualFolder = false,
                LastModified = res.LastModified,
                Id = id,
                Metadata = dic
            };
        }

        public Task InsertAsync(IBlob t)
        {
            return PutAsync(_client, _bucketName, t);
        }

        public Task UpsertAsync(IBlob t)
        {
            return InsertAsync(t);
        }

        internal static async Task PutAsync(IAmazonS3 client, string bucketName, IBlob t)
        {
            var res = await client.PutObjectAsync(new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = t.Id,
                InputStream = t.Body
            });

            if ((int)res.HttpStatusCode > 202)
            {
                throw new Amazon.S3.AmazonS3Exception($"Call to insert {t.Id} returned {res.HttpStatusCode}");
            }

        }

    }
}
