using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BeeHive.Azure
{
    internal static class CloudBlockBlobExtensions
    {
        public static GenericBufferedStream ToStream(this CloudBlockBlob blob)
        {
            Func<long, byte[], int> filler = (remoteOffset, buffer) =>
            {
                var read = blob.DownloadRangeToByteArray(buffer, 0, remoteOffset, buffer.Length);
                return read;
            };

            return new GenericBufferedStream(blob.Properties.Length, filler);
        }
    }
}
