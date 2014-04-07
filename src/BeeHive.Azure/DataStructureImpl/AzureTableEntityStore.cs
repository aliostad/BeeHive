using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using Microsoft.WindowsAzure.Storage.Table;

namespace BeeHive.Azure
{
    public class AzureTableEntityStore<T> : ICollectionStore<T>
        where T : TableEntity, IHaveIdentity
    {
        public Task<T> GetAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task InsertAsync(T t)
        {
            throw new NotImplementedException();
        }

        public Task UpsertAsync(T t)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(T t)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
