using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{
    public interface IKeyedListStore<T>
    {
        Task AddAsync(Guid key, T t);

        Task<IEnumerable<T>> GetAsync(Guid key);

        Task RemoveAsync(Guid key);

        Task<bool> ExistsAsync(Guid key);
    }

    public interface ISimpleKeyedListStore : IKeyedListStore<Guid>
    {
       
    }


}
