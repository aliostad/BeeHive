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
        
        private ConcurrentDictionary<string, ConcurrentDictionary<Guid,T>> _store = new ConcurrentDictionary<string, ConcurrentDictionary<Guid, T>>();
        private bool _isConcurrencyAware = false;

        public InMemoryKeyedListStore()
        {
            _isConcurrencyAware = typeof (T) is IConcurrencyAware;
        }

        private ConcurrentDictionary<Guid, T> GetList(string listName)
        {
            return _store.GetOrAdd(listName, new ConcurrentDictionary<Guid, T>());
        }


        public async Task AddAsync(string listName, Guid key, T t)
        {
            var list = GetList(listName);

            if (!list.TryAdd(key, t))
                throw new KeyAlreadyExistsException(key);
 
        }

        public async Task<IEnumerable<T>> GetAsync(string listName, Guid key)
        {
            
            ConcurrentDictionary<Guid,T> bag;
            if (!_store.TryGetValue(listName, out bag))
                throw new KeyNotFoundException(listName);
            return bag.Values.ToArray();

        }


        public async Task RemoveAsync(string listName, Guid key)
        {
            ConcurrentDictionary<Guid, T> bag;           
            _store.TryRemove(listName, out bag);
        }

        public async Task<bool> ExistsAsync(string listName, Guid key)
        {
            if (!_store.ContainsKey(listName))
                return false;
            var list = GetList(listName);
            return list.ContainsKey(key);
        }

        public async Task UpdateAsync(string listName, Guid key, T t)
        {
            var list = GetList(listName);
            
            // first check if item exist
            if(!list.ContainsKey(key))
                throw new KeyNotFoundException(key.ToString());

            // then do an update with concurrency check. Unfortunately does not allow for doing any other way
            var result = list.AddOrUpdate(key, default(T), (k, old) =>
            {
                if (_isConcurrencyAware)
                {
                    var oldcw = old as IConcurrencyAware;
                    var tcw = t as IConcurrencyAware;
                    tcw.AssertNoConflict(oldcw);
                }
                return t;
            });

        }
    }
}
