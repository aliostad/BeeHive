using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{
    public interface IKeyedListStore<T>
        where T : IHaveIdentity
    {
        Task AddAsync(string listName, Guid key, T t);

        Task<IEnumerable<T>> GetAsync(string listName, Guid key);

        Task RemoveAsync(string listName, Guid key);

        Task<bool> ExistsAsync(string listName, Guid key);

        Task UpdateAsync(string listName, Guid key, T t);

    }

}
