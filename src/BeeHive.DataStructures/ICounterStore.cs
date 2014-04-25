using System;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{
    public interface ICounterStore
    {
        Task<long> GetAsync(string counterName, string id);

        /// <summary>
        /// Use it for decrement as well
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task IncrementAsync(string counterName, string id, long value);
    }
}

