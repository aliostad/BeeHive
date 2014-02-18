using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using BeeHive.DataStructures;

namespace BeeHive.ConsoleDemo
{
    public class InMemoryCounterStore : ICounterStore
    {
        private ConcurrentDictionary<Guid, long> _store = new ConcurrentDictionary<Guid, long>();

        public async Task<long> GetAsync(Guid id)
        {
            return _store.GetOrAdd(id, 0);
        }

        public async Task IncrementAsync(Guid id, long value)
        {
            _store.AddOrUpdate(id, value, (g, v) => v + value);
        }
    }
}
