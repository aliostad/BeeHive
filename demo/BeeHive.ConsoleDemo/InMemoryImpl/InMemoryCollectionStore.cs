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
        private ConcurrentDictionary<Type, ConcurrentDictionary<string, T>> _store = new ConcurrentDictionary<Type, ConcurrentDictionary<string, T>>();

        private bool _isConcurrencyAware = false;
        public InMemoryCollectionStore()
        {
            _isConcurrencyAware = typeof(T) is IConcurrencyAware;
        }

        private ConcurrentDictionary<string, T> GetList()
        {
            return _store.GetOrAdd(typeof (T), new ConcurrentDictionary<string, T>());
        }

        public async Task<T> GetAsync(string id)
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

        public async Task DeleteAsync(T t)
        {

            var list = GetList();
            if (_isConcurrencyAware)
            {
                var old = default(T);
                if (list.TryGetValue(t.Id, out old))
                {
                    var oldcw = old as IConcurrencyAware;
                    var tcw = t as IConcurrencyAware;
                    tcw.AssertNoConflict(oldcw);
                }
            }

            list.TryRemove(t.Id, out t);
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var list = GetList();
            return list.ContainsKey(id);
        }
    }
}
