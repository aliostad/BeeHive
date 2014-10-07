using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{
    public interface IBigTableStore<T>
        where T : IHaveIdentityAndRange, new()
    {
        Task<T> GetAsync(string id, string rangeKey);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">if id is null then returns all values</param>
        /// <param name="rangeStart"></param>
        /// <param name="rangeEnd"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetRangeAsync(string id, string rangeStart, string rangeEnd);

        Task InsertAsync(T t);

        Task UpsertAsync(T t);

        Task DeleteAsync(T t);

        Task<bool> ExistsAsync(string id, string rangeKey);
    }

    public static class IBigTableStoreExtensions
    {
        public static Task<IEnumerable<T>> GetAllAsync<T>(this IBigTableStore<T> table, string id)
            where T : IHaveIdentityAndRange, new()
        {
            return table.GetRangeAsync(id, "0", "zzzzzzzz");
        }

        public static Task<IEnumerable<T>> GetAllAsync<T>(this IBigTableStore<T> table)
            where T : IHaveIdentityAndRange, new()
        {
            return table.GetRangeAsync(null, "0", "zzzzzzzz");
        }
    }
}
