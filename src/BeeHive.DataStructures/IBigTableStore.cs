using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{
    public interface IBigTableStore<T>
        where T : IHaveIdentityAndRange
    {
        Task<T> GetAsync(string id, string rangeKey);

        Task<IEnumerable<T>> GetRangeAsync(string id, string rangeStart, string rangeEnd);

        Task InsertAsync(T t);

        Task UpsertAsync(T t);

        Task DeleteAsync(T t);

        Task<bool> ExistsAsync(string id, string rangeKey);
    }
}
