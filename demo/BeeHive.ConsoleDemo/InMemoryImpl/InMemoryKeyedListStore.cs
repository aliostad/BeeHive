using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;

namespace BeeHive.ConsoleDemo
{
    public class InMemoryKeyedListStore<T> : IKeyedListStore<T>
    {
        
        private ConcurrentDictionary<Guid, ConcurrentBag<T>> _store = new ConcurrentDictionary<Guid, ConcurrentBag<T>>();  

        public async Task AddAsync(Guid key, T t)
        {
            if(!_store.TryAdd(key, new ConcurrentBag<T>()))
                throw new KeyAlreadyExistsException(key);
        }

        public async Task<IEnumerable<T>> GetAsync(Guid key)
        {
            ConcurrentBag<T> bag;
            if(!_store.TryGetValue(key, out bag))
                throw new KeyNotFoundException(key.ToString());
            return bag.ToArray();
        }

        public async Task RemoveAsync(Guid key)
        {
            ConcurrentBag<T> bag;
            _store.TryRemove(key, out bag);
        }

        public async Task<bool> ExistsAsync(Guid key)
        {
            return _store.ContainsKey(key);
        }
    }
}
