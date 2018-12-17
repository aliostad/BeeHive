using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BeeHive.Azure
{
    internal static class IListBlobItemExtensions
    {
        public static IBlob ToBlob(this IListBlobItem blob)
        {
            var blockBlob = blob as CloudBlockBlob;
            var directoryBlob = blob as CloudBlobDirectory;
            if (blockBlob != null)
            {
                return new SimpleBlob()
                {
                    Id = blockBlob.Name,
                    IsVirtualFolder = false,
                    ETag = blockBlob.Properties.ETag,
                    LastModified = blockBlob.Properties.LastModified,
                    UnderlyingBlob = blockBlob,
                    Metadata = null
                };
            }
            else if (directoryBlob!= null)
            {
                return new SimpleBlob()
                {
                    IsVirtualFolder = true,
                    Id = directoryBlob.Prefix,
                    Body = new MemoryStream(),
                    ETag = null,
                    LastModified = null,
                    Metadata = new Dictionary<string, string>(),
                    UnderlyingBlob = directoryBlob
                };
            }
            else
            {
                throw new NotSupportedException("blob list item not supported");
            }
            
        }
    }
}
