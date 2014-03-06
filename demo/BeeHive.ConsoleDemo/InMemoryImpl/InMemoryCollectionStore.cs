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
        where T : IHaveIdentity
    {
        private ConcurrentDictionary<Type, ConcurrentDictionary<Guid, T>> _store = new ConcurrentDictionary<Type, ConcurrentDictionary<Guid, T>>();

        private bool _isConcurrencyAware = false;
        public InMemoryCollectionStore()
        {
            _isConcurrencyAware = typeof(T) is IConcurrencyAware;
        }

        private ConcurrentDictionary<Guid, T> GetList()
        {
            return _store.GetOrAdd(typeof (T), new ConcurrentDictionary<Guid, T>());
        }

        public async Task<T> GetAsync(Guid id)
        {
            var list = GetList();
            T t;
            if(!list.TryGetValue(id, out t))
                throw new KeyNotFoundException(id.ToString());
            return t;
        }

        public async Task InsertAsync(T t)
        {
            var id = t.Id;
            var list = GetList();
            if (!list.TryAdd(id, t))
                throw new KeyAlreadyExistsException(id);
        }

        public async Task UpsertAsync(T t)
        {
            var list = GetList();
            var id = t.Id;
            list.AddOrUpdate(id, t, (g, old) =>
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

        public async Task DeleteAsync(Guid id)
        {
            T t;
            var list = GetList();
            list.TryRemove(id, out t);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            var list = GetList();
            return list.ContainsKey(id);
        }
    }
}
