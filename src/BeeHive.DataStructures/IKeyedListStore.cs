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
        Task AddAsync(string listName, string key, T t);

        Task<IEnumerable<T>> GetAsync(string listName, string key);

        Task RemoveAsync(string listName, string key);

        Task<bool> ListExistsAsync(string listName);

        Task<bool> ExistsAsync(string listName, string key);

        Task<bool> ItemExistsAsync(string listName, string key, string itemId);

        Task UpdateAsync(string listName, string key, T t);

    }

}
