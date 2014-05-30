using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{

    /// <summary>
    /// Based on Amazon's Dynamo paper
    /// </summary>
    public interface IDynamoStore : IKeyValueStore
    {
        Task<IEnumerable<IBlob>> ListAsync(string path, bool flatSearch = false);

        Task<Dictionary<string, string>> GetMetadataAsync(string id);

    }
}
