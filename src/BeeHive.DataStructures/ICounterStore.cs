using System;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{
    public interface ICounterStore
    {
        Task<long> GetAsync(Guid id);

        /// <summary>
        /// Use it for decrement as well
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task IncrementAsync(Guid id, long value);
    }
}

