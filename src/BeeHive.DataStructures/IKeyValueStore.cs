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

        Task InsertAsync(IBlob t);

        Task UpsertAsync(IBlob t);

        Task DeleteAsync(IBlob t);

        Task<bool> ExistsAsync(string id);

    }
}
