using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{
    /// <summary>
    /// This structures stores a named list for a Guid key
    /// The list can contain only IHaveIdentity items and like a dictionary 
    /// stores unique values so duplicate IHaveIdentity cannot exist
    /// The list is created and items added, items updated and then the whole list is removed 
    /// by the client when work is done.
    /// 
    /// This structure is useful for implementing Scatter-Gather pattern.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IKeyedListStore<T>
        where T : IHaveIdentity
    {
        Task AddAsync(string listName, Guid key, T t);

        Task<IEnumerable<T>> GetAsync(string listName, Guid key);

        Task RemoveAsync(string listName, Guid key);

        Task<bool> ListExistsAsync(string listName, Guid key);

        Task<bool> ExistsAsync(string listName, Guid key, Guid itemId);

        Task UpdateAsync(string listName, Guid key, T t);

    }

}
