using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{
    public interface IKeyValueStore
    {
        Task<IBlob> GetAsync(string id);

        Task<Dictionary<string, string>> GetMetadataAsync(string id);

        Task InsertAsync(IBlob t);

        Task UpsertAsync(IBlob t);

        Task DeleteAsync(IBlob t);

        Task<bool> ExistsAsync(string id);

        /// <summary>
        /// This is optional.
        /// Implementations can throw NotSuportedException.
        /// </summary>
        /// <param name="startIdPrefix">Usually start path</param>
        /// <param name="endIdPrefix">End path</param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetRangeAsync(string startIdPrefix, string endIdPrefix);

    }
}
