using System;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{
    public interface ICollectionStore<T>
        where T : IHaveIdentity
    {

        Task<T> GetAsync(Guid id);

        Task InsertAsync(T t);

        Task UpsertAsync(T t);

        Task DeleteAsync(Guid id);

        Task<bool> ExistsAsync(Guid id);

    }
}
