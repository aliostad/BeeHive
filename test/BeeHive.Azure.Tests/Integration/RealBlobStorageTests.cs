using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using Microsoft.WindowsAzure.Storage.Blob;
using Xunit;
using Xunit.Extensions;

namespace BeeHive.Azure.Tests.Integration
{

    /// <summary>
    /// NOTE: MAKE SURE!!!
    /// azure_storage_connection_string env var is set!!
    /// </summary>

    public class RealBlobStorageTests
    {
        private const string ContainerName = "beehivetest";
        private const string FileName9 = "ha/hooo/vi/voo/safa9.bin";
        private const string FileName2 = "ha/hooo/vi/voo/safa2.bin";
        private const string FileName1 = "ha/hooo/vi/voo/safa1.bin";
        private const string FileName4 = "ha/hooo/vi/voo/safa4.bin";
        private const string FileName12 = "ha/hooo/vi/voo/safa12.bin";
        private string _cn = null;

        public RealBlobStorageTests()
        {
            var cn = Environment.GetEnvironmentVariable(EnvVars.ConnectionStrings.AzureStorage);
            _cn = cn;
        }

        [EnvVarIgnoreTheory(EnvVars.ConnectionStrings.AzureStorage)]
        [InlineData(FileName1)]
        [InlineData(FileName2)]
        [InlineData(FileName4)]
        [InlineData(FileName9)]
        [InlineData(FileName12)]
        public void ReadLargeFile(string fileName)
        {
            var store = new AzureKeyValueStore(_cn, ContainerName);
            var blob = store.GetAsync(fileName).Result;
            var stream = new MemoryStream();
            blob.Body.CopyTo(stream);
            Assert.Equal( ((CloudBlockBlob)blob.UnderlyingBlob).Properties.Length, stream.Length);
        }

        [EnvVarIgnoreTheory(EnvVars.ConnectionStrings.AzureStorage)]
        [InlineData(FileName1, 150 * 1000)]
        [InlineData(FileName2, 374 * 1001)]
        [InlineData(FileName4, 659 * 997)]
        [InlineData(FileName9, 1032 * 979)]
        [InlineData(FileName12, 1731 * 979)]
        public void WriteLargeFile(string fileName, int rounds)
        {
            var store = new AzureKeyValueStore(_cn, ContainerName);
            store.InsertAsync(new SimpleBlob()
            {
                Id = fileName,
                Body = GetRandomStream(rounds)

            }).Wait();
        }

        private Stream GetRandomStream(int rounds)
        {
            var stream = new MemoryStream();
            var random = new Random();
            var writer = new StreamWriter(stream);
            for (int i = 0; i < rounds; i++)
            {
                writer.Write(random.Next().ToString());
            }

            stream.Position = 0;
            return stream;
        }
    }
}
