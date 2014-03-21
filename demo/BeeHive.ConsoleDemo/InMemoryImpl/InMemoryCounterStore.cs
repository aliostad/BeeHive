using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using BeeHive.DataStructures;

namespace BeeHive.ConsoleDemo
{
    public class InMemoryCounterStore : ICounterStore
    {
        private ConcurrentDictionary<string, long> _store = new ConcurrentDictionary<string, long>();

        public async Task<long> GetAsync(string counterName, Guid id)
        {
            string key = string.Format("{0}-{1}", counterName, id);
            return _store.GetOrAdd(key, 0);
        }

        public async Task IncrementAsync(string counterName, Guid id, long value)
        {
            string key = string.Format("{0}-{1}", counterName, id);
            _store.AddOrUpdate(key, value, (g, v) => v + value);
        }
    }
}
