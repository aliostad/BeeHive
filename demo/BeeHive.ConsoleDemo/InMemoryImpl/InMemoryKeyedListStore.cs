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

        private ConcurrentDictionary<string, ConcurrentDictionary<string, T>> _store = new ConcurrentDictionary<string, ConcurrentDictionary<string, T>>();
        private bool _isConcurrencyAware = false;

        public InMemoryKeyedListStore()
        {
            _isConcurrencyAware = typeof(IConcurrencyAware).IsAssignableFrom(typeof(T));
        }

        private ConcurrentDictionary<string, T> GetList(string listName, string key)
        {
            return _store.GetOrAdd(GetKey(listName, key), new ConcurrentDictionary<string, T>());
        }

        private string GetKey(string listName, string key)
        {
            return listName + "-" + key;
        }

        public async Task AddAsync(string listName, string key, T t)
        {
            var list = GetList(listName, key);

            if (!list.TryAdd(t.Id, t))
                throw new KeyAlreadyExistsException(t.Id);
 
        }

        public async Task<IEnumerable<T>> GetAsync(string listName, string key)
        {
            
            ConcurrentDictionary<string,T> bag;
            if (!_store.TryGetValue(GetKey(listName, key), out bag))
                throw new KeyNotFoundException(listName);
            return bag.Values.ToArray();

        }


        public async Task RemoveAsync(string listName, string key)
        {
            ConcurrentDictionary<string, T> bag;           
            _store.TryRemove(GetKey(listName, key), out bag);
        }

        public async Task<bool> ListExistsAsync(string listName)
        {
            return _store.ContainsKey(listName);
        }

        public async Task<bool> ExistsAsync(string listName, string key)
        {
            return _store.ContainsKey(GetKey(listName, key));
        }

        public async Task<bool> ListExistsAsync(string listName, string key)
        {
            return _store.ContainsKey(GetKey(listName, key));
        }

        public async Task<bool> ItemExistsAsync(string listName, string key, string itemId)
        {
            if (!_store.ContainsKey(GetKey(listName, key)))
                return false;
            var list = GetList(listName, key);
            return list.ContainsKey(itemId);
        }

        public async Task UpdateAsync(string listName, string key, T t)
        {
            var list = GetList(listName, key);
            
            // first check if item exist

            if(!list.ContainsKey(t.Id))
                throw new KeyNotFoundException(t.Id.ToString());

            // then do an update with concurrency check. Unfortunately does not allow for doing any other way
            var result = list.AddOrUpdate(t.Id, default(T), (k, old) =>
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
