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

        public async Task<bool> ExistsAsync(string id)
        {
            var res = await _client.GetObjectMetadataAsync(new GetObjectMetadataRequest()
            {
                BucketName = _bucketName,
                Key = id
            });

            return res.HttpStatusCode != HttpStatusCode.NotFound;
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

        public async Task InsertAsync(IBlob t)
        {
            var res = await _client.PutObjectAsync(new PutObjectRequest()
            {
                BucketName = _bucketName,
                Key = t.Id,
                InputStream = t.Body
            });

            if ((int)res.HttpStatusCode > 202)
            {
                throw new Amazon.S3.AmazonS3Exception($"Call to insert {t.Id} returned {res.HttpStatusCode}");
            }
        }

        public Task UpsertAsync(IBlob t)
        {
            return InsertAsync(t);
        }
    }
}
