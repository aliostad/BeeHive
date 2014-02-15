using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;

namespace BeeHive.ConsoleDemo
{
    public class InMemoryCollectionStore<T> : ICollectionStore<T>
    {
        private ConcurrentDictionary<Guid, T> _store = new ConcurrentDictionary<Guid, T>(); 

        public async Task<T> GetAsync(Guid id)
        {
            T t;
            if(!_store.TryGetValue(id, out t))
                throw new KeyNotFoundException(id.ToString());
            return t;
        }

        public async Task InsertAsync(Guid id, T t)
        {
            if (_store.TryAdd(id, t))
                throw new KeyAlreadyExistsException(id);
        }

        public async Task UpsertAsync(Guid id, T t)
        {
            _store.AddOrUpdate(id, t, (g, old) => t);
        }

        public async Task DeleteAsync(Guid id)
        {
            T t;
            _store.TryRemove(id, out t);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return _store.ContainsKey(id);
        }
    }
}
