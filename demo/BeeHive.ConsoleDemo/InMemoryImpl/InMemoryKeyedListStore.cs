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
        where T : IHaveIdentity
    {
        
        private ConcurrentDictionary<string, ConcurrentBag<T>> _store = new ConcurrentDictionary<string, ConcurrentBag<T>>();  

        public async Task AddAsync(string listName, Guid key, T t)
        {
            string k = GetKey(listName, key);
            if (!_store.TryAdd(k, new ConcurrentBag<T>()))
                throw new KeyAlreadyExistsException(k);
 
        }

        public async Task<IEnumerable<T>> GetAsync(string listName, Guid key)
        {
            string k = GetKey(listName, key);
            ConcurrentBag<T> bag;
            if (!_store.TryGetValue(k, out bag))
                throw new KeyNotFoundException(k);
            return bag.ToArray();

        }

        private string GetKey(string listName, Guid key)
        {
            return string.Format("{0}:{1}", listName, key);
        }


        public async Task RemoveAsync(string listName, Guid key)
        {
            ConcurrentBag<T> bag;
            string k = GetKey(listName, key);
            _store.TryRemove(k, out bag);
        }

        public async Task<bool> ExistsAsync(string listName, Guid key)
        {
            string k = GetKey(listName, key);
            return _store.ContainsKey(k);
        }

        public Task UpdateAsync(string listName, Guid key, T t)
        {
            var concurrencyAware = t as IConcurrencyAware;
            if (concurrencyAware != null)
            {
                
            }

            throw new NotImplementedException();
        }
    }
}
