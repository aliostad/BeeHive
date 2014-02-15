using System;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{
    public interface ICollectionStore<T>
    {

        Task<T> GetAsync(Guid id);

        Task InsertAsync(Guid id,T t);

        Task UpsertAsync(Guid id, T t);

        Task DeleteAsync(Guid id);

        Task<bool> ExistsAsync(Guid id);

    }
}
